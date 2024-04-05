using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    internal unsafe struct Filters : IDisposable
    {
        private Dependencies* _dependencies;
        private FilterUpdater _filterUpdater;
        private NDictionary<BAllocator, Mask, uint, Mask> _maskToFilters;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filters(Dependencies* dependencies, uint capacity)
        {
            _dependencies  = dependencies;
            _filterUpdater = FilterUpdater.Create(&dependencies->bAllocator, capacity);
            _maskToFilters = new NDictionary<BAllocator, Mask, uint, Mask>(&dependencies->bAllocator, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe FilterData* Create(ref Mask mask)
        {
            if (!_maskToFilters.TryGetValue(mask, out uint filterId))
            {
                var includes = mask.includes.ValuesAsSpan();
                var excludes = mask.excludes.ValuesAsSpan();

                _dependencies->archetype.Create(ref _filterUpdater, includes);

                var filterData = new FilterData(
                    _dependencies,
                    _maskToFilters.Count,
                    mask,
                    NList<BAllocator, uint>.CreateWrapper(_dependencies->archetype.Filter(&_dependencies->bAllocator, includes, excludes))
                    );
                
                filterId = _filterUpdater.filters.Count;

                _filterUpdater.filters.Add(filterData);
                _maskToFilters.Add(mask, filterId);

                foreach (var id in filterData.archetypeIds)
                {
                    _filterUpdater.archetypeIdToFilterId.Add(id, filterId);
                }
            }

            return _filterUpdater.filters.GetPtr(filterId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id, uint elementId, bool isTemporary)
            => _dependencies->archetype.Add(ref _filterUpdater, id, elementId, isTemporary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id, uint elementId)
            => _dependencies->archetype.Remove(ref _filterUpdater, id, elementId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filterUpdater.Dispose();
            _maskToFilters.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
        {
            _dependencies->archetype.Lock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            _dependencies->archetype.Unlock(ref _filterUpdater);
        }
    }

    internal unsafe struct FilterData : IDisposable
    {
        private Dependencies* _dependencies;
        private Mask _mask;
        private uint _id;

        internal NList<BAllocator, uint> archetypeIds;
        internal NArray<BAllocator, uint> entities;
        internal uint entityCount;

        public FilterData(Dependencies* dependencies, uint id, in Mask mask, in NList<BAllocator, uint> archetypeIds)
        {
            _dependencies = dependencies;
            _mask = mask;
            _id = id;

            this.archetypeIds = archetypeIds;
            entities = new NArray<BAllocator, uint>(&dependencies->bAllocator, 16);
            entityCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<TNArray>(ref TNArray archetypes, uint archetypeId, uint itemId)
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
                entityCount += _dependencies->archetype.ReadIdCollection(archetypeIds.Read(i)).Count;
            }

            if (entities.Length < entityCount)
            {
                entities.Resize(entityCount);
            }

            uint counter = 0;
            for (uint i = 0; i < archetypeIds.Length; ++i)
            {
                foreach (var id in _dependencies->archetype.ReadIdCollection(archetypeIds.Read(i)))
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
        void AddToFilterData<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, uint itemId)
           where TNArray : struct, INArray<Node>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EndToFilterData();
    }

    internal struct FilterUpdater : IDisposable, IFilterUpdater
    {
        public NMultiDictionaryZero<BAllocator, uint, uint, U4U4HashProvider> archetypeIdToFilterId;
        public NList<BAllocator, FilterData> filters;

        private NHashSetZero<BAllocator, uint, U4U4HashProvider> _temp;

        public static unsafe FilterUpdater Create(BAllocator* allocator, uint capacity)
        {
            FilterUpdater inst = default;
            inst.archetypeIdToFilterId = new NMultiDictionaryZero<BAllocator, uint, uint, U4U4HashProvider>(allocator, capacity);
            inst.filters = new NList<BAllocator, FilterData>(allocator, capacity);
            inst._temp = new NHashSetZero<BAllocator, uint, U4U4HashProvider>(allocator, capacity);
            return inst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddToFilterData<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, uint itemId)
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
            filters.DeepDispose();
        }
    }

    public struct EntityCollection
    {
        public uint id;
        public WArray<EntityId> entities;
    }
}
