using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class InjectUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Contruct<T0, A0>(ref T0 structure, A0 argument)
            where T0 : struct, IInject<A0>
        {
            structure.Construct(argument);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decontruct<T0>(ref T0 structure)
            where T0 : struct, IInject
        {
            structure.Deconstruct();
        }
    }
}