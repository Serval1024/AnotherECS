using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Jobs")]
namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe class BFilter : IFilter
    {
        private State _state;
        private FilterData* _filterData;

        internal void Construct(State state, FilterData* filterData)
        {
            _state = state;
            _filterData = filterData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref readonly IdCollection<HAllocator> GetEntities(uint archetypeId)
            => ref _state.GetEntitiesByArchetype(archetypeId);

        public IEnumerator<uint> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal State GetState()
            => _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FilterData* GetFilterData()
            => _filterData;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lock()
        {
            _state.LockFilter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Unlock()
        {
            _state.UnlockFilter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetUserData<TUserData>(uint id, TUserData data)
            where TUserData : struct, IModuleData
        {
            _state.SetModuleData(id, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TUserData GetUserData<TUserData>(uint id)
            where TUserData : struct, IModuleData
            => _state.GetModuleData<TUserData>(id);

        public struct Enumerator : IEnumerator<EntityId>
        {
            private readonly BFilter _filter;
            private readonly FilterData* _filterData;

            private int _current;
            private NHashSet<HAllocator, uint, U4U4HashProvider>.Enumerator _currentCollection;

            public Enumerator(BFilter filter)
            {
                _filter = filter;
                _filterData = filter._filterData;
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
                if (_currentCollection.IsValide && _currentCollection.MoveNext())
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

    public class Filter<T0> : BFilter
        where T0 : IComponent
    { }

    public class Filter<T0, T1> : BFilter
        where T0 : IComponent
        where T1 : IComponent
    { }

    public class Filter<T0, T1, T2> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    { }

    public class Filter<T0, T1, T2, T3> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    { }

    public class Filter<T0, T1, T2, T3, T4> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    { }

    public class Filter<T0, T1, T2, T3, T4, T5> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
    { }

    public class Filter<T0, T1, T2, T3, T4, T5, T6> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
    { }

    public class Filter<T0, T1, T2, T3, T4, T5, T6, T7> : BFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
    { }
}
