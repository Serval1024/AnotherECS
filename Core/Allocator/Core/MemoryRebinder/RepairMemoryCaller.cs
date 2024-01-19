using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class RepairMemoryCaller
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Repair<T>(ref T data, ref RepairMemoryContext repairMemoryContext)
            where T : struct, IRepairMemoryHandle
        {
            data.RepairMemoryHandle(ref repairMemoryContext);
        }
    }
}
