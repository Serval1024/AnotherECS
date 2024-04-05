using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public static class InjectUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Construct<TStruct, T0>(ref TStruct structure, T0 argument)
            where TStruct : struct, IInject<T0>
        {
            structure.Construct(argument);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Construct<TStruct, T0, T1>(ref TStruct structure, T0 argument0, T1 argument1)
            where TStruct : struct, IInject<T0, T1>
        {
            structure.Construct(argument0, argument1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<TStruct>(ref TStruct structure)
            where TStruct : struct, IInject
        {
            structure.Deconstruct();
        }
    }
}