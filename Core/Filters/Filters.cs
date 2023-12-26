using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    internal unsafe struct Filters : IDisposable
    {
        private GlobalDepencies* _depencies;
        private FilterUpdater _filterUpdater;
        private NDictionary<BAllocator, Mask, uint, Mask> _maskTofilters;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filters(GlobalDepencies* depencies, uint capacity)
        {
            _depencies  = depencies;
            _filterUpdater = FilterUpdater.Create(&depencies->bAllocator, capacity);
            _maskTofilters = new NDictionary<BAllocator, Mask, uint, Mask>(&depencies->bAllocator, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe FilterData* Create(ref Mask mask)
        {
            if (!_maskTofilters.TryGetValue(mask, out uint filterId))
            {
                var includes = mask.includes.ValuesAsSpan();
                var excludes = mask.excludes.ValuesAsSpan();

                _depencies->archetype.Create(ref _filterUpdater, includes);

                var filterData = new FilterData(
                    _depencies,
                    _maskTofilters.Count,
                    mask,
                    NList<BAllocator, uint>.CreateWrapper(_depencies->archetype.Filter(&_depencies->bAllocator, includes, excludes))
                    );
                
                filterId = _filterUpdater.filters.Count;

                _filterUpdater.filters.Add(filterData);
                _maskTofilters.Add(mask, filterId);

                foreach (var id in filterData.archetypeIds)
                {
                    _filterUpdater.archetypeIdToFilterId.Add(id, filterId);
                }
            }

            return _filterUpdater.filters.GetPtr(filterId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id, ushort elementId, bool isTemporary)
            => _depencies->archetype.Add(ref _filterUpdater, id, elementId, isTemporary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id, ushort elementId)
            => _depencies->archetype.Remove(ref _filterUpdater, id, elementId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filterUpdater.Dispose();
            _maskTofilters.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
        {
            _depencies->archetype.Lock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            _depencies->archetype.Unlock(ref _filterUpdater);
        }
    }

    internal unsafe struct FilterData : IDisposable
    {
        private GlobalDepencies* _depencies;
        private Mask _mask;
        private uint _id;

        internal NList<BAllocator, uint> archetypeIds;
        internal NArray<BAllocator, uint> entities;
        internal uint entityCount;

        public FilterData(GlobalDepencies* depencies, uint id, in Mask mask, in NList<BAllocator, uint> archetypeIds)
        {
            _depencies = depencies;
            _mask = mask;
            _id = id;

            this.archetypeIds = archetypeIds;
            entities = new NArray<BAllocator, uint>(&depencies->bAllocator, 16);
            entityCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<TNArray>(ref TNArray archetypes, uint archetypeId, ushort itemId)
            where TNArray : struct, INArray<Node>
        {
            if (!_mask.excludes.Contains(itemId))
            {
                ArchetypeComparer<TNArray> comparer = default;
                comparer.archetypes = archetypes;
                archetypeIds.AddSort(ref comparer, archetypeId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityCollection GetEntities()
        {
            Update();
            return new EntityCollection() { id = _id, entities = new WArray<EntityId>(entities.ReadPtr(), entityCount) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            entityCount = 0;
            for (uint i = 0; i < archetypeIds.Length; ++i)
            {
                entityCount += _depencies->archetype.ReadIdCollection(archetypeIds.Read(i)).Count;
            }

            if (entities.Length < entityCount)
            {
                entities.Resize(entityCount);
            }

            uint counter = 0;
            for (uint i = 0; i < archetypeIds.Length; ++i)
            {
                foreach (var id in _depencies->archetype.ReadIdCollection(archetypeIds.Read(i)))
                {
                    entities.GetRef(counter++) = id;
                }
            }
        }

        public void Dispose()
        {
            archetypeIds.Dispose();
            entities.Dispose();
        }


        private struct ArchetypeComparer<TNArray> : IComparer<uint>
             where TNArray : struct, INArray<Node>
        {
            public TNArray archetypes;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(uint x, uint y)
            {
                var hash0 = archetypes.ReadRef(x).hash;
                var hash1 = archetypes.ReadRef(y).hash;
                if (hash0 > hash1)
                {
                    return 1;
                }
                else if (hash0 < hash1)
                {
                    return -1;
                }
                return 0;
            }
        }

    }


    internal interface IFilterUpdater
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToFilterData<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, ushort itemId)
           where TNArray : struct, INArray<Node>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EndToFilterData();
    }

    internal struct FilterUpdater : IDisposable, IFilterUpdater
    {
        public NMultiDictionary<BAllocator, uint, uint, U4U4HashProvider> archetypeIdToFilterId;
        public NList<BAllocator, FilterData> filters;

        private NHashSet<BAllocator, uint, U4U4HashProvider> _temp;

        public static unsafe FilterUpdater Create(BAllocator* allocator, uint capacity)
        {
            FilterUpdater inst = default;
            inst.archetypeIdToFilterId = new NMultiDictionary<BAllocator, uint, uint, U4U4HashProvider>(allocator, capacity);
            inst.filters = new NList<BAllocator, FilterData>(allocator, capacity);
            inst._temp = new NHashSet<BAllocator, uint, U4U4HashProvider>(allocator, capacity);
            return inst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddToFilterData<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, ushort itemId)
            where TNArray : struct, INArray<Node>
        {
            foreach (var filterId in archetypeIdToFilterId.GetValues(archetypeId))
            {
                if (!_temp.Contains(filterId))
                {
                    _temp.Add(filterId);
                    filters.GetRef(filterId).Add(ref archetypes, toAddArchetypeId, itemId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndToFilterData()
        {
            _temp.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            archetypeIdToFilterId.Dispose();
            filters.Dispose();
        }
    }

    public struct EntityCollection
    {
        public uint id;
        public WArray<EntityId> entities;
    }
}
