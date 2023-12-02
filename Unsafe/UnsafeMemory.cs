using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unsafe
{
    public unsafe static class UnsafeMemory     //TODO SER +crossplatform
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocateReplace<T>(T replace)
            where T : unmanaged
        {
            var ptr = Allocate<T>();
            *ptr = replace;
            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>()
            where T : unmanaged
            => (T*)Allocate(sizeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>(uint count)
            where T : unmanaged
            => (T*)Allocate(count * sizeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Allocate(long size)
        {
            var ptr = Malloc(size);
            MemClear(ptr, size);
            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeDeallocate<T>(ref T* ptr)
            where T : unmanaged, IDisposable
        {
            ptr->Dispose();
            Deallocate(ref ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deallocate<T>(ref T* ptr)
            where T : unmanaged
        {
            if (ptr != null)
            {
                Free(ptr);
                ptr = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deallocate(ref void* ptr)
        {
            if (ptr != null)
            {
                Free(ptr);
                ptr = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(void* destination, long size)
        {
            if (destination != null)
            {
                UnsafeUtility.MemClear(destination, size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Malloc(long size)
           => Malloc(size, 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Malloc(long size, int alignment)
            => UnsafeUtility.Malloc(size, alignment, Unity.Collections.Allocator.Persistent);
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* memory)
        {
            UnsafeUtility.Free(memory, Unity.Collections.Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemClear(void* destination, long size)
        {
            UnsafeUtility.MemClear(destination, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCopy(void* destination, void* source, long size)
        {
            UnsafeUtility.MemCpy(destination, source, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Copy(void* source, long size)
        {
            var copy = Malloc(size);
            MemCopy(copy, source, size);
            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetElementArray<T>(void* source, int index)
            where T : unmanaged
            => ref *(((T*)source) + index);
    }
}