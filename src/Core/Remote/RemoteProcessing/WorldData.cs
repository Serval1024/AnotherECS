using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public readonly struct WorldData
    {
        public readonly IWorldExtend World;
        public readonly State State;

        public WorldData(object data)
        {
            World = default;
            State = default;

            if (data is IWorldExtend worldExtend)
            {
                World = worldExtend;
            }
            else if (data is State state)
            {
                State = state;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WorldData(World world)
            => new(world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WorldData(State state)
            => new(state);
    }
}
