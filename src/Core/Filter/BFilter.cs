using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Jobs")]
[assembly: InternalsVisibleTo("AnotherECS.Unity.Physics")]
namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe class BFilter
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal State GetState()
            => _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntityCollection GetEntities()
            => _filterData->GetEntities();

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
    }
}
