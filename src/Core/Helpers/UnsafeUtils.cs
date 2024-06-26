using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unsafe
{
    internal unsafe static class UnsafeUtils
    {
        public static string AsArrayToString<T>(T* ptr, int count)
            where T : unmanaged
        {
            if (ptr != null)
            {
                var result = new StringBuilder();
                for (int i = 0; i < count; ++i)
                {
                    result.Append(UnsafeMemory.ReadElementArray<T>(ptr, i).ToString());
                    result.Append(" ,");
                }
                return result.ToString();
            }
            return "null";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* AddressOf<T>(ref T output)
            where T : struct
#if UNITY_5_3_OR_NEWER
            => UnsafeUtility.AddressOf(ref output);
#else
            => System.Runtime.CompilerServices.Unsafe.AsPointer(ref output);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T As<U, T>(ref U from)
#if UNITY_5_3_OR_NEWER
            => ref UnsafeUtility.As<U, T>(ref from);
#else
            => ref System.Runtime.CompilerServices.Unsafe.As<U, T>(ref from);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf(Type type)
#if UNITY_5_3_OR_NEWER
            => UnsafeUtility.SizeOf(type);
#else
            => System.Runtime.InteropServices.Marshal.SizeOf(type);
#endif
    }
}