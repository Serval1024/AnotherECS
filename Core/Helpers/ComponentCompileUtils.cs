using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public static class ComponentCompileUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Construct<T0, A0>(ref T0 structure, A0 argument)
           where T0 : struct, IInject<A0>
        {
            structure.Construct(argument);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T0>(ref T0 structure)
            where T0 : struct, IInject
        {
            structure.Deconstruct();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RepairStateId<T>(ref T structure, ushort stateId)
            where T : struct, IRepairStateId
        { 
            structure.RepairStateId(stateId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RepairMemoryHandle<T>(ref T structure, ref RepairMemoryContext repairMemoryContext)
            where T : struct, IRepairMemoryHandle
        {
            structure.RepairMemoryHandle(ref repairMemoryContext);
        }
    }
}