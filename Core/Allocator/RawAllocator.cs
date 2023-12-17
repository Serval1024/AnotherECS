using System.Runtime.CompilerServices;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    public unsafe struct RawAllocator : IAllocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryHandle Allocate(uint size)
            => new() { pointer = UnsafeMemory.Allocate(size) };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(ref MemoryHandle memoryHandle)
        {
            Deallocate(ref memoryHandle.pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(ref void* pointer)
        {
            UnsafeMemory.Deallocate(ref pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(void* pointer)
        {
            Deallocate(ref pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reuse(ref MemoryHandle memoryHandle, uint size) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(ref MemoryHandle memoryHandle) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResize(ref MemoryHandle memoryHandle, uint size)
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges(ref MemoryHandle memoryHandle) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges(ref MemoryHandle memoryHandle)
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repair(ref MemoryHandle memoryHandle) { }
    }
}

