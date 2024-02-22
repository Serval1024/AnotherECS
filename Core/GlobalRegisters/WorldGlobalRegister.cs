using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public static class WorldGlobalRegister
    {
        private static IdRegister<World> _impl = new();

        public static ushort Register(World world)
            => _impl.Register(world);

        public static void Unregister(ushort id)
        {
            _impl.Unregister(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World Get(ushort id)
            => _impl.Get(id);
    }
}