using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Unsafe
{
    public unsafe static class UnsafeMemory
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
#if UNITY_5_3_OR_NEWER
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(destination, size);
#else
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Malloc(long size)
           => Malloc(size, 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Malloc(long size, int alignment)
#if UNITY_5_3_OR_NEWER
            => Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, alignment, Unity.Collections.Allocator.Persistent);
#else
            => System.Runtime.InteropServices.Marshal.AllocHGlobal((int)size).ToPointer();
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* memory)
        {
#if UNITY_5_3_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(memory, Unity.Collections.Allocator.Persistent);
#else
            System.Runtime.InteropServices.Marshal.FreeHGlobal(new IntPtr(memory));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemClear(void* destination, long size)
        {
#if UNITY_5_3_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(destination, size);
#else
            MemSet(destination, 0, size);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemSet(void* destination, byte value, long size)
        {
#if UNITY_5_3_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSet(destination, value, size);
#else
            var ptr = (byte*)destination;
            for (int i = 0; i < size; ++i)
            {
                ptr[i] = value;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCopy(void* destination, void* source, long size)
        {
#if UNITY_5_3_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(destination, source, size);
#else
            Buffer.MemoryCopy(source, destination, size, size);
#endif
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