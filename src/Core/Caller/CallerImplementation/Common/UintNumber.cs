using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct UintNumber : INumberProvier<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Next(uint number)
            => ++number;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToNumber(uint number)
            => number;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToGeneric(uint number)
            => number;
    }
}
