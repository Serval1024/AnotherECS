using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public static class StateGlobalRegister
    {
        private static IdRegister<State> _impl = new();

        public static ushort Register(State state)
            => _impl.Register(state);

        public static void Unregister(ushort id)
        {
            _impl.Unregister(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Get(ushort id)
            => _impl.Get(id);
    }
}