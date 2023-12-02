using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal struct Filters : IDisposable
    {
        private ArchetypeCaller _archetype;
        private FilterUpdater _filterUpdater;
        private NDictionary<Mask, uint, Mask> _maskTofilters;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filters(ArchetypeCaller caller, uint capacity)
        {
            _archetype = caller;
            _filterUpdater = FilterUpdater.Create(capacity);
            _maskTofilters = new NDictionary<Mask, uint, Mask>(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe FilterData* Create(ref Mask mask)
        {
            if (!_maskTofilters.TryGetValue(mask, out uint filterId))
            {
                var includes = mask.includes.ValuesAsSpan();
                var excludes = mask.excludes.ValuesAsSpan();

                _archetype.Create(ref _filterUpdater, includes);

                var filterData = new FilterData()
                {
                    mask = mask,
                    archetypeIds = NList<uint>.CreateWrapper(_archetype.Filter(includes, excludes))
                };

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
            => _archetype.Add(ref _filterUpdater, id, elementId, isTemporary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id, ushort elementId)
            => _archetype.Remove(ref _filterUpdater, id, elementId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filterUpdater.Dispose();
            _maskTofilters.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
        {
            _archetype.Lock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            _archetype.Unlock(ref _filterUpdater);
        }
    }

    internal struct FilterData : IDisposable
    {
        public Mask mask;
        public NList<uint> archetypeIds;

        public void Add(ref NArray<Node> archetypes, uint archetypeId, ushort itemId)
        {
            if (!mask.excludes.Contains(itemId))
            {
                ArchetypeComparer comparer = default;
                comparer.archetypes = archetypes;
                archetypeIds.AddSort(ref comparer, archetypeId);
            }
        }

        public void Dispose()
        {
            archetypeIds.Dispose();
        }


        private struct ArchetypeComparer : IComparer<uint>
        {
            public NArray<Node> archetypes;

            public int Compare(uint x, uint y)
            {
                var hash0 = archetypes.GetRef(x).hash;
                var hash1 = archetypes.GetRef(y).hash;
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

    internal struct FilterUpdater : IDisposable
    {
        public NMultiDictionary<uint, uint, U4U4HashProvider> archetypeIdToFilterId;
        public NList<FilterData> filters;

        private NHashSet<uint, U4U4HashProvider> _temp;

        public static FilterUpdater Create(uint capacity)
        {
            FilterUpdater inst = default;
            inst.archetypeIdToFilterId = new NMultiDictionary<uint, uint, U4U4HashProvider>(capacity);
            inst.filters = new NList<FilterData>(capacity);
            inst._temp = new NHashSet<uint, U4U4HashProvider>(capacity);
            return inst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddToFilterData(ref NArray<Node> archetypes, uint archetypeId, uint toAddArchetypeId, ushort itemId)
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

}
