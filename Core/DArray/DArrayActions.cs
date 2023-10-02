using System.Runtime.CompilerServices;
using static AnotherECS.Core.DArrayCaller;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class DArrayActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateLayout(ref UnmanagedLayout<Container> layout, ref GlobalDepencies depencies)
        {
            MultiStorageActions<Container>.AllocateRecycle(ref layout, depencies.config.general.recycledCapacity);
            MultiStorageActions<Container>.AllocateDense(ref layout, depencies.config.general.dArrayCapacity);

            MultiHistoryFacadeActions<Container>.AllocateRecycle(ref layout, ref depencies);
            MultiHistoryFacadeActions<Container>.AllocateDense(ref layout, ref depencies);
        }
    }
}

