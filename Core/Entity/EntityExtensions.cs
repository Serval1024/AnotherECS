using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public static class EntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity ToEntity(this State state, EntityId id)
            => new()
            {
                id = id,
                generation = state.GetGeneration(id),
                stateId = state.GetStateId(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToEntityId(this in Entity entity, out EntityId id)
            => entity.TryToEntityId(out var _, out id) ? id : throw new Exceptions.CanNotCastException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToEntityId(this in Entity entity, out EntityId id)
            => entity.TryToEntityId(out var _, out id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToEntityId(this in Entity entity, out State state, out EntityId id)
        {
            if (!entity.State.IsHas(entity.id, entity.generation))
            {
                state = null;
                id = 0;
                return false;
            }

            state = entity.State;
            id = entity.id;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in Entity a, in Entity b)
            => a.id == b.id && a.generation == b.generation && a.stateId == b.stateId;
    }
}