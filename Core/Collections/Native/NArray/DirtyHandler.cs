using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public unsafe struct DirtyHandler<TAllocator>
        where TAllocator : unmanaged, IAllocator
    {
        private TAllocator* _allocator;
        private MemoryHandle _memoryHandle;

        public DirtyHandler(TAllocator* allocator, MemoryHandle memoryHandle)
        {
            _allocator = allocator;
            _memoryHandle = memoryHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty()
        {
            _allocator->Dirty(ref _memoryHandle);
        }
    }
}