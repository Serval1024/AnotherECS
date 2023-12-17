using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Debug;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    internal unsafe struct ChangeHistory : IDisposable, ISerialize
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

        public ChangeHistory(BAllocator* allocator, uint capacity, uint recordHistoryLength)
        {
            _meta = new NArray<BAllocator, Meta>(allocator, 32);
            _buffer = new NArray<BAllocator, byte>(allocator, capacity);
            _recordHistoryLength = recordHistoryLength;
            _current = 0;
            _isNeedRefreshReference = false;
            _notReadyToUse = default;
        }

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
        public unsafe bool RevertTo(ref HAllocator source, uint tick)
        {
            if (_isNeedRefreshReference)
            {
                return RevertTo<TrueConst>(ref source, tick);
            }
            else
            {
                _ = RevertTo<FalseConst>(ref source, tick);
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool RevertTo<TIsBrokenReferenceInBuffer>(ref HAllocator source, uint tick)
            where TIsBrokenReferenceInBuffer : struct, IBoolConst
        {
            bool isNotNeedRefreshReference = true;

            for (uint i = _current; i >= 0; --i)
            {
                ref var frame = ref _meta.GetRef(i);

                if (frame.tick > tick)
                {
                    RestoreMemory(
                        source.GetChunk(frame.destChunk).GetPointerBySegment(frame.destSegment),
                        _buffer.GetPtr(frame.bufferIndex),
                        frame.size);

                    if (default(TIsBrokenReferenceInBuffer).Is)
                    {
                        if (i < _notReadyToUse.Length)
                        {
                            isNotNeedRefreshReference &= _notReadyToUse.Get(i);
                            _notReadyToUse.Set(i, true);
                        }
                    }
                }
                else
                {
                    _current = i;

                    if (default(TIsBrokenReferenceInBuffer).Is)
                    {
                        TryDropRefreshReference();
                        return !isNotNeedRefreshReference;
                    }
                    return false;
                }
            }

            for (uint i = _meta.Length - 1; i > _current; --i)
            {
                ref var frame = ref _meta.GetRef(i);

                if (frame.tick > tick)
                {
                    RestoreMemory(
                            source.GetChunk(frame.destChunk).GetPointerBySegment(frame.destSegment),
                            _buffer.GetPtr(frame.bufferIndex),
                            frame.size);

                    if (default(TIsBrokenReferenceInBuffer).Is)
                    {
                        if (i < _notReadyToUse.Length)
                        {
                            isNotNeedRefreshReference &= _notReadyToUse.Get(i);
                            _notReadyToUse.Set(i, true);
                        }
                    }
                }
                else
                {
                    _current = i;

                    if (default(TIsBrokenReferenceInBuffer).Is)
                    {
                        TryDropRefreshReference();
                        return !isNotNeedRefreshReference;
                    }
                    return false;
                }
            }

            if (default(TIsBrokenReferenceInBuffer).Is)
            {
                TryDropRefreshReference();
                return !isNotNeedRefreshReference;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryDropRefreshReference()
        {
            for (uint i = 0; i < _notReadyToUse.Length; ++i)
            {
                if (!_notReadyToUse.Get(i))
                {
                    return;
                }
            }
            _isNeedRefreshReference = false;
            _notReadyToUse.Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _meta.Dispose();
            _buffer.Dispose();
            _notReadyToUse.Dispose();
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

        public void Pack(ref WriterContextSerializer writer)
        {
            _isNeedRefreshReference = true;
            _notReadyToUse = new NArray<BAllocator, bool>(writer.GetDepency<NPtr<BAllocator>>().Value, _meta.Length);

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