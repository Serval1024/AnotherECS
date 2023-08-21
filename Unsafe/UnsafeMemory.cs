using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unsafe
{
    public unsafe static class UnsafeMemory
    {
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
        public static void MemCpy(void* destination, void* source, long size)
        {
            UnsafeUtility.MemCpy(destination, source, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Copy(void* source, long size)
        {
            var copy = Malloc(size);
            MemCpy(copy, source, size);
            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetElementArray<T>(void* source, int index)
            where T : unmanaged
            => ref *(((T*)source) + index);

        public static string ToString<T>(void* ptr, int count)
            where T : unmanaged
        {
            if ((IntPtr)ptr != IntPtr.Zero)
            {
                var result = new StringBuilder();
                for (int i = 0; i < count; ++i)
                {
                    result.Append(GetElementArray<T>(ptr, i).ToString());
                    result.Append(" ,");
                }
                return result.ToString();
            }
            return "null";
        }
    }
}