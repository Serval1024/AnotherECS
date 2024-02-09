using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unity.Jobs
{
    public static unsafe class NativeArrayUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(void* ptr, uint length)
            where T : unmanaged
            => NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, (int)length, Allocator.None);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<TAllocator, T>(NArray<TAllocator, T> narray)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
            => ToNativeArray<T>(narray.GetPtr(), narray.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(WArray<T> warray)
           where T : unmanaged
           => ToNativeArray<T>(warray.GetPtr(), warray.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CopyNativeArray<T>(void* ptr, uint length)
            where T : unmanaged
        {
            var nativeArray = new NativeArray<T>((int)length, Allocator.Persistent);
            UnsafeUtility.MemCpy(nativeArray.GetUnsafePtr(), ptr, length * sizeof(T));
            return nativeArray;
        }
    }
}