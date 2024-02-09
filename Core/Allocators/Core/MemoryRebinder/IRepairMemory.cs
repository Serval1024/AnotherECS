using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Allocators
{
    public interface IRepairMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Repair(ref MemoryHandle memoryHandle);
    }
}
