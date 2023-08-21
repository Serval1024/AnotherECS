using EntityId = System.Int32;
using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal class Entities : ISerializeConstructor
    {
        internal const ushort AllocateGeneration = 32768;

        private ushort[] _data;
        private int _count;
        private int _segmentSize;
        private int _gcEntityCheckPerTick;

        private int[] _recycled;
        private int _recycledCount;
        private int _currentIndexGC;

#if ANOTHERECS_HISTORY_DISABLE
        internal Entities(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
        }
#else
        private readonly EntitiesHistory _history;

        internal Entities(ref ReaderContextSerializer reader, EntitiesHistory history)
        {
            _history = history;
            Unpack(ref reader);
        }
#endif

#if ANOTHERECS_HISTORY_DISABLE
        public Entities(in GeneralConfig config)
#else
        public Entities(in GeneralConfig config, EntitiesHistory history)
#endif
        {
            _segmentSize = IndexOffset.BeginComponent + (int)config.componentPerEntityCapacity;
            _data = new ushort[config.entityCapacity * _segmentSize];
            _count = 1;

            _recycled = new int[config.recycledCapacity];
            _recycledCount = 0;

            _gcEntityCheckPerTick = (int)config.gcEntityCheckPerTick * _segmentSize;
            _currentIndexGC = IndexOffset.ComponentCount;
#if !ANOTHERECS_HISTORY_DISABLE
            _history = history;
#endif
        }

        public ushort this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ushort GetRef(int index)
            => ref _data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id < _count && IsHasRaw(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => GetGeneration(id) >= AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentCount(EntityId id)
            => _data[GetOffsetSegment(id) + IndexOffset.ComponentCount];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponent(EntityId id, int index)
            => _data[GetOffsetSegment(id) + IndexOffset.BeginComponent + index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<ushort> GetComponents(EntityId id)
        {
            var offset = GetOffsetSegment(id);
            var count = _data[offset + IndexOffset.ComponentCount];
            return new ArraySegment<ushort>(_data, offset + IndexOffset.BeginComponent, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetGeneration(EntityId id)
            => _data[GetOffsetSegment(id) + IndexOffset.Generation];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntityCapacity()
            => _data.Length / _segmentSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRawCount()
            => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAllocatedCount()
            => GetRawCount() - _recycledCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCount()
            => GetAllocatedCount() - 1;

        public int GetAlives(ref EntityId[] entities)
        {
            var count = GetCount();
            if (entities == null || entities.Length < count)
            {
                entities = new int[count];
            }

            var id = 0;
            for (int i = 1, offset = _segmentSize; i < _count; ++i, offset += _segmentSize)
            {
                if (_data[offset + IndexOffset.Generation] >= AllocateGeneration)
                {
                    entities[id++] = i;
                }
            }
            return count;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffsetSegment(EntityId id)
            => id * _segmentSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ushort componentType)
        {
            var offset = GetOffsetSegment(id);
            var offsetComponentCount = offset + IndexOffset.ComponentCount;
            ref var count = ref _data[offsetComponentCount];
            var saveIndex = offset + count + IndexOffset.BeginComponent;
            if (saveIndex == _segmentSize)
            {
                /*
                _segmentSize = SegmentUtils.Resize(GetEntityCapacity(), _segmentSize, _segmentSize + 4, _count * _segmentSize, ref _data);
#if ANOTHERECS_DEBUG
                Logger.SegmentResized(_segmentSize - 4, _segmentSize);
#endif
                */
                throw new Exceptions.ReachedLimitEntityException(_segmentSize);
            }

#if !ANOTHERECS_HISTORY_DISABLE
            _history.PushArrayElement(offsetComponentCount, count);
            _history.PushArrayElement(saveIndex, 0);
#endif
            _data[saveIndex] = componentType;
            ++count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id, ushort componentType)
        {
            var offset = GetOffsetSegment(id);
            var offsetComponentCount = offset + IndexOffset.ComponentCount;

            ref var dataCount = ref _data[offsetComponentCount];
#if !ANOTHERECS_HISTORY_DISABLE
            _history.PushArrayElement(offsetComponentCount, dataCount);
#endif
            --dataCount;

            var dataOffset = offset + IndexOffset.BeginComponent;
            for (var i = 0; i <= dataCount; i++)
            {
                if (_data[dataOffset + i] == componentType)
                {
                    if (i < dataCount)
                    {
                        _data[dataOffset + i] = _data[dataOffset + dataCount];
                    }
#if !ANOTHERECS_HISTORY_DISABLE
                    _history.PushArrayElement(dataOffset + i, _data[dataOffset + i]);
#endif
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate(out int newSize)
        {
            newSize = -1;
            int id;
            if (_recycledCount > 0)
            {
#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushRecycledCount(_recycledCount);
#endif
                id = _recycled[--_recycledCount];
                var offset = GetOffsetSegment(id);

                ref var generation = ref _data[offset + IndexOffset.Generation];
                generation += AllocateGeneration + 1;
                if (generation == ushort.MaxValue)
                {
                    generation = AllocateGeneration;
                }

                _data[offset + IndexOffset.ComponentCount] = 0;
            }
            else
            {
                if (_count * _segmentSize == _data.Length)
                {
                    newSize = _count << 1;
                    Array.Resize(ref _data, newSize * _segmentSize);
                }

                id = _count++;
                var offset = GetOffsetSegment(id);
                var offsetGeneration = offset + IndexOffset.Generation;
                ref var generation = ref _data[offsetGeneration];
#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushArrayElement(offsetGeneration, generation);
#endif
                generation = AllocateGeneration;
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
            => Deallocate(id, GetOffsetSegment(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id, int offset)
        {
            if (_recycledCount == _recycled.Length)
            {
                Array.Resize(ref _recycled, _recycledCount << 1);
            }

            var offsetGeneration = offset + IndexOffset.Generation;
            ref var generation = ref _data[offsetGeneration];


#if !ANOTHERECS_HISTORY_DISABLE
            _history.PushArrayElement(offsetGeneration, generation);
            _history.PushRecycled(_recycled[_recycledCount], _recycledCount);

            var count = _data[offset + IndexOffset.ComponentCount];
            if (count != 0)
            {
                var offsetBeginComponent = _data[offset + IndexOffset.BeginComponent];

                for (int i = offsetBeginComponent, iMax = offsetBeginComponent + count; i < iMax; ++i)
                {
                    _history.PushArrayElement(i, _data[i]);
                }
            }
#endif
            _data[offsetGeneration] -= AllocateGeneration;
            _recycled[_recycledCount++] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRecycledCountRaw(int value)
         => _recycledCount = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetRecycledRaw()
            => _recycled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort[] GetArrayRaw()
            => _data;

        public void TickFinished()
        {
            if (_gcEntityCheckPerTick != 0)
            {
                InterationGarbageCollect(_gcEntityCheckPerTick);
            }
        }

        public void GarbageCollect()
        {
            var offset = IndexOffset.Generation - IndexOffset.ComponentCount;
            for (int i = IndexOffset.ComponentCount; i < _data.Length; i += _segmentSize)
            {
                if (_data[i] == 0 && _data[i + offset] >= AllocateGeneration)
                {
                    Deallocate((i - IndexOffset.ComponentCount) / _segmentSize);
                }
            }
        }
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(_data);
            writer.Write(_count);
            writer.Write(_segmentSize);
            writer.Write(_gcEntityCheckPerTick);

            writer.Pack(_recycled);
            writer.Write(_recycledCount);
            writer.Write(_currentIndexGC);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data = reader.Unpack<ushort[]>();
            _count = reader.ReadInt32();
            _segmentSize = reader.ReadInt32();
            _gcEntityCheckPerTick = reader.ReadInt32();

            _recycled = reader.Unpack<EntityId[]>();
            _recycledCount = reader.ReadInt32();
            _currentIndexGC = reader.ReadInt32();
        }


        private void InterationGarbageCollect(int offsetIndex)
        {
            var iMax = _currentIndexGC + offsetIndex;
            if (iMax > _data.Length)
            {
                iMax = _data.Length;
            }

            var offset = IndexOffset.Generation - IndexOffset.ComponentCount;
            for (int i = _currentIndexGC; i < iMax; i += _segmentSize)
            {
                if (_data[i] == 0 && _data[i + offset] >= AllocateGeneration)
                {
                    Deallocate((i - IndexOffset.ComponentCount) / _segmentSize);
                }
            }

            _currentIndexGC = (_currentIndexGC + offsetIndex) % _data.Length;
        }



        public static class IndexOffset
        {
            public const int ComponentCount = 0;
            public const int Generation = 1;
            public const int BeginComponent = 2;
        }
    }
}
