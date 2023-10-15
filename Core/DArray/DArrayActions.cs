using System.Runtime.CompilerServices;
using static AnotherECS.Core.DArrayCaller;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class DArrayActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateLayout(ref UnmanagedLayout<DArrayContainer> layout, ref GlobalDepencies depencies)
        {
            MultiStorageActions<DArrayContainer>.AllocateRecycle(ref layout, depencies.config.general.recycledCapacity);
            MultiStorageActions<DArrayContainer>.AllocateDense(ref layout, depencies.config.general.dArrayCapacity);

            MultiHistoryFacadeActions<DArrayContainer>.AllocateRecycle(ref layout, ref depencies);
            MultiHistoryFacadeActions<DArrayContainer>.AllocateDense(ref layout, ref depencies);
        }
    }
}

