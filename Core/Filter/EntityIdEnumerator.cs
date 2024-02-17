using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public unsafe struct EntityIdEnumerator : IEnumerator<EntityId>
    {
        private readonly BEntityIdFilter _filter;
        private readonly FilterData* _filterData;

        private int _current;
        private NHashSetZero<HAllocator, uint, U4U4HashProvider>.Enumerator _currentCollection;

        public EntityIdEnumerator(BEntityIdFilter filter)
        {
            _filter = filter;
            _filterData = filter.GetFilterData();
            _currentCollection = default;
            _current = -1;
            _filter.Lock();
        }

        public EntityId Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentCollection.Current;
        }

        object IEnumerator.Current
            => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_currentCollection.IsValid && _currentCollection.MoveNext())
            {
                return true;
            }
            else
            {
                while (++_current < _filterData->archetypeIds.Count)
                {
                    ref readonly var idSet = ref _filter.GetEntities(_filterData->archetypeIds.Get(_current));

                    if (idSet.Count != 0)
                    {
                        _currentCollection = idSet.GetEnumerator();
                        _currentCollection.MoveNext();
                        return true;
                    }
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _filter.Unlock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _current = -1;
        }
    }
}
