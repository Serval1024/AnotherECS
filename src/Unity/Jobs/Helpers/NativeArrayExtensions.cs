using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unity.Jobs
{
    public static unsafe class NativeArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetRef<T>(this NativeArray<T> nativeArray, int index)
            where T : unmanaged
            => ref UnsafeUtility.ArrayElementAsRef<T>(nativeArray.GetUnsafePtr(), index);
    }
}