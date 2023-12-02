using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct UshortNumber : INumberProvier<ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Next(uint number)
            => (ushort)++number;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConverToDI(ushort number)
            => number;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConverToNumber(uint number)
            => (ushort)number;
    }
}
