using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class EntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Pack(this State state, int id)
            => new()
            {
                id = id,
                generation = state.GetGeneration(id),
                state = state,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in Entity packed, out int id)
            => packed.Unpack(out var _, out id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in Entity packed, out State state, out int id)
        {
            if (packed.state == null || !packed.state.IsHas(packed.id, packed.generation))
            {
                state = null;
                id = -1;
                return false;
            }

            state = packed.state;
            id = packed.id;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in Entity a, in Entity b)
            => a.id == b.id && a.generation == b.generation && a.state == b.state;
    }
}