namespace AnotherECS.Core
{
    public unsafe interface IHistory
    {
        uint ParallelMax { get; set; }
        IHistory Create(BAllocator* allocator, uint historyCapacity, uint recordHistoryLength);
        void Push(ref MemoryHandle memoryHandle, uint size);
        bool RevertTo(ref HAllocator hAllocator, uint tick);
        void TickFinished();
    }
}