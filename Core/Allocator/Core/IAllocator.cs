namespace AnotherECS.Core
{
    public interface IAllocator
    {
        bool IsValid { get; }
        uint GetId();
        MemoryHandle Allocate(uint size);
        void Deallocate(ref MemoryHandle memoryHandle);
        public void Reuse(ref MemoryHandle memoryHandle, uint size);
        void Dirty(ref MemoryHandle memoryHandle);
        bool TryResize(ref MemoryHandle memoryHandle, uint size);
        void Repair(ref MemoryHandle memoryHandle);
        void EnterCheckChanges(ref MemoryHandle memoryHandle);
        bool ExitCheckChanges(ref MemoryHandle memoryHandle);
    }
}

