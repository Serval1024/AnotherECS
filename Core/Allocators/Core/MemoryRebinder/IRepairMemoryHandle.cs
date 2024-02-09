namespace AnotherECS.Core.Allocators
{
    public interface IRepairMemoryHandle
    {
        void RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext);
    }
}
