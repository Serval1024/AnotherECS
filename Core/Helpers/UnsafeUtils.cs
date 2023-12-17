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
            if ((IntPtr)ptr != IntPtr.Zero)
            {
                var result = new StringBuilder();
                for (int i = 0; i < count; ++i)
                {
                    result.Append(UnsafeMemory.GetElementArray<T>(ptr, i).ToString());
                    result.Append(" ,");
                }
                return result.ToString();
            }
            return "null";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* AddressOf<T>(ref T output)
            where T : struct
            => UnsafeUtility.AddressOf(ref output);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T As<U, T>(ref U from)
            => ref UnsafeUtility.As<U, T>(ref from);
    }
}