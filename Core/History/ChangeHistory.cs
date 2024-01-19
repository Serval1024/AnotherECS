using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Debug;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    internal unsafe struct ChangeHistory : IHistory, IDisposable, ISerialize
    {
        private NArray<BAllocator, Meta> _meta;
        private NArray<BAllocator, byte> _buffer;
        private uint _recordHistoryLength;
        private uint _current;
        private uint _tick;
        private NeedRefreshByTick _reference;

        public ulong TotalBytesSaved
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint count = 0;
                for(uint i = 0; i < _meta.Length; ++i)
                {
                    count += _meta.GetRef(i).size;
                }
                return count;
            }
        }

        public uint ParallelMax { get => 1; set { } }

        public ChangeHistory(BAllocator* allocator, uint capacity, uint recordHistoryLength)
        {
            _meta = new NArray<BAllocator, Meta>(allocator, 32);
            _buffer = new NArray<BAllocator, byte>(allocator, capacity);
            _recordHistoryLength = recordHistoryLength;
            _current = 0;
            _tick = 0;
            _reference = default;
        }

        public IHistory Create(BAllocator* allocator, uint historyCapacity, uint recordHistoryLength)
            => new ChangeHistory(allocator, historyCapacity, recordHistoryLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(ref MemoryHandle memoryHandle, uint size)
        {
            ref var zeroMeta = ref _meta.GetRef(0);
            if (_tick > _recordHistoryLength + zeroMeta.tick)
            {
                var index = GetStartMemoryEnough(size);
                if (index != uint.MaxValue)
                {
                    if (_tick > _recordHistoryLength + _meta.GetRef(index).tick)
                    {
                        for (uint i = 0; i <= index; ++i)
                        {
                            _meta.GetRef(i) = default;
                        }
                        _current = 0;
                    }
                }
            }

            ref var meta = ref _meta.GetRef(_current);
            var dataIndex = meta.bufferIndex + meta.size;

            var endIndex = dataIndex + size;
            if (endIndex > _buffer.Length)
            {
                _buffer.Resize((endIndex > _buffer.Length << 1) ? endIndex : (_buffer.Length << 1));
#if !ANOTHERECS_RELEASE
                Logger.HistoryBufferResized("Data buffer", _buffer.Length);
#endif
            }

            ++_current;
            if (_current == _meta.Length)
            {
                _meta.Resize(_meta.Length << 1);
#if !ANOTHERECS_RELEASE
                Logger.HistoryBufferResized("Meta buffer", _meta.Length);
#endif
            }

            ref var newMeta = ref _meta.GetRef(_current);
            newMeta.tick = _tick;
            newMeta.size = size;
            newMeta.bufferIndex = dataIndex;
            newMeta.destId = memoryHandle.id;

            SaveMemory(dataIndex, memoryHandle.pointer, newMeta.size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool RevertTo(ref HAllocator destination, uint tick)
        {
            RevertToInternal(ref destination, tick);
            return _reference.IsActive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RevertToInternal(ref HAllocator destination, uint tick)
        {
            for (int i = (int)_current; i >= 0; --i)
            {
                ref var frame = ref _meta.GetRef(i);

                if (frame.tick > tick)
                {
                    IndexRevertTo(ref destination, ref frame);
                }
                else
                {
                    _current = (uint)i;
                    return;
                }
            }

            for (int i = (int)_meta.Length - 1; i > _current; --i)
            {
                ref var frame = ref _meta.GetRef(i);

                if (frame.tick > tick)
                {
                    IndexRevertTo(ref destination, ref frame);
                }
                else
                {
                    _current = (uint)i;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryStepBack(ref HAllocator destination, uint tick)
        {
            ref var frame = ref _meta.GetRef(_current);
            if (_current == 0)
            {
                _current = _meta.Length;
            }
            else
            {
                --_current;
            }

            if (frame.tick > tick)
            {
                IndexRevertTo(ref destination, ref frame);
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IndexRevertTo(ref HAllocator destination, ref Meta frame)
        {
            RestoreMemory(
                        destination.GetPointerById(frame.destId),
                        _buffer.GetPtr(frame.bufferIndex),
                        frame.size
                        );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDropRefreshReference()
            => _reference.TryDrop(_tick);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _meta.Dispose();
            _buffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStarted(uint tick)
        {
            _tick = tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            TryDropRefreshReference();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _meta.PackBlittable(ref writer);
            _buffer.PackBlittable(ref writer);
            writer.Write(_recordHistoryLength);
            writer.Write(_tick);
            writer.Write(_current);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _meta.UnpackBlittable(ref reader);
            _buffer.UnpackBlittable(ref reader);
            _recordHistoryLength = reader.ReadUInt32();
            _tick = reader.ReadUInt32();
            _current = reader.ReadUInt32();

            _reference.Set(_tick);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveMemory(uint offset, void* memory, uint size)
        {
            UnsafeMemory.MemCopy(_buffer.GetPtr() + offset, memory, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RestoreMemory(void* destination, void* source, uint size)
        {
            UnsafeMemory.MemCopy(destination, source, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetStartMemoryEnough(uint size)
        {
            uint summary = 0;
            for (uint i = 0; i < _meta.Length; ++i)
            {
                summary += _meta.GetRef(i).size;
                if (summary >= size)
                {
                    return i;
                }
            }
            return uint.MaxValue;
        }


        private struct Meta
        {
            public uint tick;
            public uint size;
            public uint bufferIndex;
            public uint destId;
        }
    }
}