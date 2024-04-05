using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Allocators
{
    public unsafe struct RepairMemoryContext
    {
        private readonly IRepairMemory[] _repairs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RepairMemoryContext(IRepairMemory[] repairs)
        {
            _repairs = repairs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repair(uint allocatorId, ref MemoryHandle memoryHandle)
        {
            _repairs[allocatorId].Repair(ref memoryHandle);
        }
    }
}
