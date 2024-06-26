using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Allocators
{
    public unsafe struct HAllocator : IAllocator, IDisposable, ISerialize
    {
        private const int CHUNK_PREALLOCATION_COUNT = 1;
        private const int SEGMENT_POWER_2 = 7;
        private const int SEGMENT_SIZE_BYTE = 1 << SEGMENT_POWER_2;
        private const uint SEGMENT_LIMIT = ushort.MaxValue;

        private BAllocator* _allocator;
        private NArray<BAllocator, Chunk> _chunks;

        private uint _id;
        private uint _chunkAllocated;
        private uint _multiplier;

        private uint _tick;

#if !ANOTHERECS_HISTORY_DISABLE
        private ChangeHistory _history;
#endif
#if !ANOTHERECS_RELEASE
        private MemoryChecker<BAllocator> _memoryChecker;
#endif

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _chunks.IsValid;
        }

        public uint ChunkLimit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _chunks.Length;
        }

        public uint SegmentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SEGMENT_SIZE_BYTE;
        }
        public uint ChunkDownBound
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 1;
        }

        public uint ChunkCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _chunkAllocated;
        }

        public ulong BytesAllocatedTotal
        {
            get
            {
                ulong result = 0;
                for (uint i = ChunkDownBound; i < ChunkCount; ++i)
                {
                    result += _chunks.ReadRef(i).MemoryTotal;
                }
                return result;
            }
        }

        public ulong HistoryBytesAllocatedTotal
#if !ANOTHERECS_HISTORY_DISABLE
            => _history.BytesAllocatedTotal;
#else
            => 0;
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId()
            => _id;

        public HAllocator(BAllocator* allocator, uint id, uint chunkLimit, uint historyCapacity, uint recordHistoryLength)
        {
            if (chunkLimit < 2)
            {
                chunkLimit = 2;
            }

            const uint chunkPreallocation = CHUNK_PREALLOCATION_COUNT + 1;

            _id = id;
            _allocator = allocator;
            _chunks = new NArray<BAllocator, Chunk>(_allocator, chunkLimit);
            _multiplier = 9;

            _chunkAllocated = chunkPreallocation;

#if !ANOTHERECS_RELEASE
            _memoryChecker = new MemoryChecker<BAllocator>(_allocator);
#endif

            _tick = 0;
#if !ANOTHERECS_HISTORY_DISABLE
            _history = (ChangeHistory)default(ChangeHistory).Create(_allocator, historyCapacity, recordHistoryLength);
#endif
            for (uint i = ChunkDownBound; i < chunkPreallocation; ++i)
            {
                _chunks.GetRef(i).Allocate(_allocator, GetSegmentNewSize());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStarted(uint tick)
        {
            _tick = tick;
#if !ANOTHERECS_HISTORY_DISABLE
            SetDirty(true);
            _history.TickStarted(tick);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
#if !ANOTHERECS_HISTORY_DISABLE
            _history.TickFinished();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropDirty()
        {
            for (uint i = ChunkDownBound; i < ChunkCount; ++i)
            {
                DropDirty(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropDirty(uint chunk)
        {
            _chunks.ReadRef(chunk).DropDirty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(ref MemoryHandle memoryHandle)
        {
#if !ANOTHERECS_HISTORY_DISABLE
            if (*memoryHandle.isNotDirty)
            {
                *memoryHandle.isNotDirty = false;

                _history.Push(
                    ref memoryHandle,
                    GetSegmentCountBySegment(memoryHandle.id) << SEGMENT_POWER_2
                    );
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Chunk GetChunk(uint chunk)
            => ref _chunks.ReadRef(chunk);


        public byte* GetPointerById(uint id)
        {
            var (chunkId, segmentId) = ToChunkSegment(id);
            return GetChunk(chunkId).GetPointerBySegment(segmentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryHandle Allocate(uint size)
        {
#if !ANOTHERECS_RELEASE
            if (size == 0)
            {
                throw new ArgumentException(nameof(size));
            }
#endif
            var segmentCount = GetSegmentCountBySize(size);

            var location = FindLocation(segmentCount);
#if !ANOTHERECS_RELEASE
            if (location.chunk == 0)
            {
                throw new Exceptions.ReachedLimitChunkException(ChunkLimit);
            }

            if (segmentCount >= SEGMENT_LIMIT)
            {
                throw new Exceptions.ReachedLimitAmountOfSegmentException(SEGMENT_LIMIT);
            }
#endif
            LockLocation(location, segmentCount);

            ref var chunk = ref _chunks.GetRef(location.chunk);

            var memoryHandle = new MemoryHandle()
            {
                isNotDirty = chunk.GetPointerDirtyBySegment(location.segment),
                pointer = chunk.GetPointerBySegment(location.segment),
                id = ToId(location.chunk, location.segment),
            };

            _allocator->Reuse(ref memoryHandle, segmentCount << SEGMENT_POWER_2);

            return memoryHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(ref MemoryHandle memoryHandle)
        {
            if (memoryHandle.IsValid)
            {
                Dirty(ref memoryHandle);
                var (chunkId, segmentId) = ToChunkSegment(memoryHandle.id);
                UnlockLocation(new Location() { chunk = chunkId, segment = segmentId });
                memoryHandle = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reuse(ref MemoryHandle memoryHandle, uint size)
        {
            Dirty(ref memoryHandle);
            UnsafeMemory.Clear(memoryHandle.pointer, size);
        }

        public bool TryResize(ref MemoryHandle memoryHandle, uint size)
        {
            var requirementSegmentCount = GetSegmentCountBySize(size);

            var (chunkId, segmentId) = ToChunkSegment(memoryHandle.id);
            var currentSegmentCount = GetSegmentCountBySegment(chunkId, segmentId);

            if (requirementSegmentCount > currentSegmentCount)
            {
                ref var chunk = ref _chunks.GetRef(chunkId);
                var deltaCount = requirementSegmentCount - currentSegmentCount;

                if (chunk.NextFree(segmentId + currentSegmentCount, deltaCount))
                {
                    chunk.LockSegments(segmentId + currentSegmentCount, deltaCount);
                    chunk.SetSegmentCount(segmentId, requirementSegmentCount);

                    UnsafeMemory.Clear(((byte*)memoryHandle.GetPtr()) + (currentSegmentCount << SEGMENT_POWER_2), deltaCount << SEGMENT_POWER_2);
                    return true;
                }

                return false;
            }
            else if (requirementSegmentCount != currentSegmentCount)
            {
                ref var chunk = ref _chunks.GetRef(chunkId);
                chunk.UnlockSegments(segmentId + requirementSegmentCount, currentSegmentCount - requirementSegmentCount);
                chunk.SetSegmentCount(segmentId, requirementSegmentCount);
            }

            Dirty(ref memoryHandle);
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges(ref MemoryHandle memoryHandle)
        {
#if !ANOTHERECS_RELEASE
            _memoryChecker.EnterCheckChanges(ref memoryHandle);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges(ref MemoryHandle memoryHandle)
#if !ANOTHERECS_RELEASE
            => _memoryChecker.ExitCheckChanges(ref memoryHandle);
#else
            => false;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RevertTo(uint tick)
#if !ANOTHERECS_RELEASE
            => _history.RevertTo(ref this, tick);
#else
            => throw new NotSupportedException();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            for (uint i = ChunkDownBound; i < _chunkAllocated; ++i)
            {
                _chunks.GetRef(i).Dispose();
            }
#if !ANOTHERECS_RELEASE
            _history.Dispose();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repair(ref MemoryHandle memoryHandle)
        {
            var (chunkId, segmentId) = ToChunkSegment(memoryHandle.id);
            ref var chunk = ref _chunks.GetRef(chunkId);
            memoryHandle.pointer = chunk.GetPointerBySegment(segmentId);
            memoryHandle.isNotDirty = chunk.GetPointerDirtyBySegment(segmentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_allocator->GetId());
            writer.Write(_id);
            writer.Write(_chunkAllocated);
            writer.Write(_multiplier);
            writer.Write(_tick);

            _chunks.Pack(ref writer);
#if !ANOTHERECS_RELEASE
            _history.Pack(ref writer);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            var allocatorId = reader.ReadUInt32();
            _allocator = reader.Dependency.DirectGet<WPtr<BAllocator>>(allocatorId).Value;
#if !ANOTHERECS_RELEASE
            _memoryChecker = new MemoryChecker<BAllocator>(_allocator);
#endif
            _id = reader.ReadUInt32();
            _chunkAllocated = reader.ReadUInt32();
            _multiplier = reader.ReadUInt32();
            _tick = reader.ReadUInt32();

            _chunks.Unpack(ref reader);

#if !ANOTHERECS_RELEASE
            _history.Unpack(ref reader);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RepairMemory<HAllocator> GetRepairMemory()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDirty(bool isNotDirty)
        {
            for (uint i = ChunkDownBound; i < _chunkAllocated; ++i)
            {
                _chunks.GetRef(i).SetDirty(isNotDirty);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ToId(uint chunk, uint segment)
            => (chunk << 16) | segment;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ToId(ushort chunk, ushort segment)
            => ((uint)chunk << 16) | segment;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (ushort chunk, ushort segment) ToChunkSegment(uint id)
            => ((ushort)(id >> 16), (ushort)(id & 0xffff));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetSegmentCountBySegment(uint id)
        {
            var (chunkId, segmentId) = ToChunkSegment(id);
            return _chunks.GetRef(chunkId).GetSegmentCount(segmentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetSegmentCountBySegment(uint chunk, uint segment)
            => _chunks.GetRef(chunk).GetSegmentCount(segment);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetSegmentCountBySize(uint size)
            => ((size - 1) >> SEGMENT_POWER_2) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location FindLocation(uint segmentCount)
        {
            for (uint i = ChunkDownBound; i < _chunkAllocated; ++i)
            {
                var segment = _chunks.GetRef(i).FindSegment(segmentCount);
                if (segment != 0)
                {
                    return new() { chunk = i, segment = segment };
                }
            }


            return new() { chunk = NewChunk(segmentCount), segment = 1 };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint NewChunk(uint segmentCount)
        {
            if (_chunkAllocated == _chunks.Length)
            {
                throw new Exceptions.ReachedLimitChunkException(ChunkLimit);
            }

            if (++_multiplier > 16)
            {
                _multiplier = 16;
            }

            var size = GetSegmentNewSize();
            if (segmentCount > size)
            {
                _chunks.GetRef(_chunkAllocated).Allocate(_allocator, segmentCount);
            }
            else
            {
                _chunks.GetRef(_chunkAllocated).Allocate(_allocator, size);
                --_multiplier;
            }

            return _chunkAllocated++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LockLocation(Location location, uint count)
        {
            ref var chunk = ref _chunks.GetRef(location.chunk);
            chunk.LockSegments(location.segment, count);
            chunk.SetSegmentCount(location.segment, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnlockLocation(Location location)
        {
            ref var chunk = ref _chunks.GetRef(location.chunk);
            chunk.UnlockSegments(location.segment, GetSegmentCountBySegment(location.chunk, location.segment));
            chunk.SetSegmentCount(location.segment, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetSegmentNewSize()
            => SEGMENT_LIMIT >> (16 - (int)_multiplier);


        private struct Location
        {
            public uint chunk;
            public uint segment;
        }

        public struct Chunk : IDisposable, ISerialize
        {
            private NArray<BAllocator, bool> _isDirty;
            private NArray<BAllocator, ushort> _sizeSegments;
            private NArray<BAllocator, bool> _freeSegments;
            private NArray<BAllocator, byte> _memory;
            private uint _freeSegmentSizeMax;
            private uint _startSearch;
            private uint _segmentUpBound;

            public uint SegmentDownBound
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => 1;
            }

            public uint SegmentUpBound
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _segmentUpBound; 
            }

            public ulong MemoryTotal
                => _isDirty.ByteLength + _sizeSegments.ByteLength + _freeSegments.ByteLength + _memory.ByteLength;

            public void Allocate(BAllocator* allocator, uint capacity)
            {
                _isDirty = new NArray<BAllocator, bool>(allocator, capacity + 1);
                _sizeSegments = new NArray<BAllocator, ushort>(allocator, capacity + 1);
                _freeSegments = new NArray<BAllocator, bool>(allocator, capacity + 1);
                _memory = new NArray<BAllocator, byte>(allocator, (capacity + 1) << SEGMENT_POWER_2);
                _freeSegmentSizeMax = uint.MaxValue;
                _segmentUpBound = SegmentDownBound;
                _startSearch = SegmentDownBound;

                _freeSegments.SetAll(SegmentDownBound, _freeSegments.Length - SegmentDownBound, true);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool* GetIsDirtyPtr()
                => _isDirty.GetPtr();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ushort* GetSizeSegmentsPtr()
                => _sizeSegments.GetPtr();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte* GetMemoryPtr()
                => _memory.GetPtr();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ushort GetSegmentCount(uint segment)
                => _sizeSegments.GetRef(segment);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSegmentCount(uint segment, uint count)
            {
                _sizeSegments.GetRef(segment) = (ushort)count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint FindSegment(uint size)
            {
                if (size <= _freeSegmentSizeMax)
                {
                    int foundCount = 0;
                    for (uint i = _startSearch; i < _freeSegments.Length; ++i)
                    {
                        foundCount = _freeSegments.Get(i) ? (foundCount + 1) : 0;
                        if (foundCount == size)
                        {
                            if (i == _startSearch + size - 1)
                            {
                                _startSearch = i + 1;
                            }
                            return i + 1 - size;
                        }
                    }
                    _freeSegmentSizeMax = size - 1;
                }
                return 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LockSegments(uint index, uint count)
            {
                uint iMax = index + count;
                for (uint i = index; i < iMax; ++i)
                {
                    _freeSegments.GetRef(i) = false;
                }

                if (_segmentUpBound < iMax)
                {
                    _segmentUpBound = iMax;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnlockSegments(uint index, uint count)
            {
                if (_startSearch > index)
                {
                    _startSearch = index;
                }

                uint iMax = index + count;
                for (uint i = index; i < iMax; ++i)
                {
                    _freeSegments.GetRef(i) = true;
                }

                if (_freeSegmentSizeMax < count)
                {
                    _freeSegmentSizeMax = count;
                }

                if (_segmentUpBound == iMax)
                {
                    for (uint i = index - 1; i > 0; --i)
                    {
                        if (!_freeSegments.GetRef(i))
                        {
                            _segmentUpBound = i + 1;
                            break;
                        }
                    }
                }
            }

            public bool NextFree(uint start, uint count)
            {
                var iMax = start + count;
                if (iMax > _freeSegments.Length)
                {
                    return false;
                }

                for (uint i = start; i < iMax; ++i)
                {
                    if (!_freeSegments.Get(i))
                    {
                        return false;
                    }
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte* GetPointerBySegment(uint segment)
                => _memory.GetPtr(segment << SEGMENT_POWER_2);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetSegmentByPointer(void* pointer)
                => (uint)(((byte*)pointer) - _memory.GetPtr()) >> SEGMENT_POWER_2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool* GetPointerDirtyBySegment(uint segment)
                => _isDirty.GetPtr(segment);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DropDirty()
            {
                SetDirty(true);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetDirty(bool isNotDirty)
            {
                _isDirty.SetAllByte(SegmentDownBound, SegmentUpBound - SegmentDownBound, isNotDirty ? (byte)1 : (byte)0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _isDirty.Dispose();
                _sizeSegments.Dispose();
                _freeSegments.Dispose();
                _memory.Dispose();
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                _isDirty.PackBlittable(ref writer);
                _sizeSegments.PackBlittable(ref writer);
                _freeSegments.PackBlittable(ref writer);
                _memory.PackBlittable(ref writer);

                writer.Write(_freeSegmentSizeMax);
                writer.Write(_startSearch);
                writer.Write(_segmentUpBound);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                _isDirty.UnpackBlittable(ref reader);
                _sizeSegments.UnpackBlittable(ref reader);
                _freeSegments.UnpackBlittable(ref reader);
                _memory.UnpackBlittable(ref reader);

                _freeSegmentSizeMax = reader.ReadUInt32();
                _startSearch = reader.ReadUInt32();
                _segmentUpBound = reader.ReadUInt32();
            }
        }
    }
}