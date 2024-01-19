using AnotherECS.Core.Collection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public unsafe struct EntityEnumerator : IEnumerator<Entity>
    {
        private readonly BEntityFilter _filter;
        private readonly FilterData* _filterData;
        private Entity _prototype;

        private int _current;
        private NHashSet<HAllocator, uint, U4U4HashProvider>.Enumerator _currentCollection;

        public EntityEnumerator(BEntityFilter filter)
        {
            _filter = filter;
            _filterData = filter.GetFilterData();
            _currentCollection = default;
            _current = -1;
            _prototype = default;
            _prototype.stateId = _filter.GetState().GetStateId();
            _prototype.generation = Entities.AllocateGeneration;

            _filter.Lock();
        }

        public Entity Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _prototype.id = _currentCollection.Current;
#if !ANOTHERECS_RELEASE
                _prototype.generation = _filter.GetState().GetGeneration(_prototype.id);
#endif           
                return _prototype;
            }
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
