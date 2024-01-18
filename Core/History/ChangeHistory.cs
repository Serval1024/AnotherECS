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
        private bool _isNeedRefreshReference;
        private NArray<BAllocator, bool> _notReadyToUse;

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
            _isNeedRefreshReference = false;
            _notReadyToUse = default;
        }

        public IHistory Create(BAllocator* allocator, uint historyCapacity, uint recordHistoryLength)
            => new ChangeHistory(allocator, historyCapacity, recordHistoryLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(uint tick, ref MemoryHandle memoryHandle, uint size)
        {
            ref var zeroMeta = ref _meta.GetRef(0);
            if (tick > _recordHistoryLength + zeroMeta.tick)
            {
                var index = GetStartMemoryEnough(size);
                if (index != uint.MaxValue)
                {
                    if (tick > _recordHistoryLength + _meta.GetRef(index).tick)
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
            newMeta.tick = tick;
            newMeta.size = size;
            newMeta.bufferIndex = dataIndex;
            newMeta.destChunk = memoryHandle.chunk;
            newMeta.destSegment = memoryHandle.segment;

            SaveMemory(dataIndex, memoryHandle.pointer, newMeta.size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool RevertTo(ref HAllocator destination, uint tick)
        {
            RevertToInternal(ref destination, tick);
            return _isNeedRefreshReference;
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
                        destination.GetChunk(frame.destChunk).GetPointerBySegment(frame.destSegment),
                        _buffer.GetPtr(frame.bufferIndex),
                        frame.size
                        );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDropRefreshReference()
        {
            if (_isNeedRefreshReference)
            {
                for (uint i = 0; i < _notReadyToUse.Length; ++i)
                {
                    if (!_notReadyToUse.Get(i))
                    {
                        return false;
                    }
                }
                _isNeedRefreshReference = false;
                _notReadyToUse.Dispose();
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeedDropRefreshReference()
            => _isNeedRefreshReference;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _meta.Dispose();
            _buffer.Dispose();
            _notReadyToUse.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            TryDropRefreshReference();  //TODO SER
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _isNeedRefreshReference = true;

            _meta.PackBlittable(ref writer);
            _buffer.PackBlittable(ref writer);
            writer.Write(_recordHistoryLength);
            writer.Write(_current);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _meta.UnpackBlittable(ref reader);
            _buffer.UnpackBlittable(ref reader);
            _recordHistoryLength = reader.ReadUInt32();
            _current = reader.ReadUInt32();

            _notReadyToUse = new NArray<BAllocator, bool>(_meta.GetAllocator(), _meta.Length);
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
            public uint destChunk;
            public uint destSegment;
        }
    }
}