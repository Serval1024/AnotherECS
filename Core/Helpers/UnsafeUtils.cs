using AnotherECS.Core;
using System;
using System.Reflection.Emit;
using System.Text;

namespace AnotherECS.Unsafe
{
    internal unsafe static class UnsafeUtils
    {
        public unsafe static void ValidateConvertToPointer(Delegate function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }
            var method = function.Method;

            if (function.Target is { } || !method.IsStatic || method is DynamicMethod)
            {
                throw new ArgumentException(nameof(function));
            }
        }

        public unsafe static delegate*<State, int, bool> ConvertToPointer(Func<State, int, bool> function)
        {
            UnsafeUtils.ValidateConvertToPointer(function);
            return (delegate*<State, int, bool>)function.Method.MethodHandle.GetFunctionPointer();
        }

        public static delegate*<ref T, ref InjectContainer, void> ConvertToPointer<T>(InjectDelegate<T> function)
            where T : struct
        {
            ValidateConvertToPointer(function);
            return (delegate*<ref T, ref InjectContainer, void>)function.Method.MethodHandle.GetFunctionPointer();
        }

        public static delegate*<State, int, void> ConvertToPointer(Action<State, int> function)
        {
            ValidateConvertToPointer(function);
            return (delegate*<State, int, void>)function.Method.MethodHandle.GetFunctionPointer();
        }

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
    }
}