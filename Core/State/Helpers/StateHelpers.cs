using AnotherECS.Core.Caller;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class StateHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount(ICaller[] callers, int startIndex, Func<ICaller, bool> rule)
        {
            var result = 0;
            for (int i = startIndex; i < callers.Length; ++i)
            {
                result += rule(callers[i]) ? 1 : 0;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CacheInit<T, U>(ICaller[] callers, int startIndex, ref T[] array, Func<ICaller, bool> rule, Func<U, int, T> assign)
        {
            array = new T[GetCount(callers, startIndex, rule)];
            for (int i = startIndex; i < callers.Length; ++i)
            {
                if (rule(callers[i]) && callers[i] is U @interface)
                {
                    array[i - startIndex] = assign(@interface, i);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CacheInit<T>(ICaller[] callers, int startIndex, ref T[] array, Func<ICaller, bool> rule)
        {
            array = new T[GetCount(callers, startIndex, rule)];

            for (int i = startIndex; i < callers.Length; ++i)
            {
                if (rule(callers[i]) && callers[i] is T @interface)
                {
                    array[i - startIndex] = @interface;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CacheInit(ICaller[] callers, int startIndex, ref Dictionary<Type, ICaller> data)
        {
            data = new Dictionary<Type, ICaller>();
            for (int i = startIndex; i < callers.Length; ++i)
            {
                data.Add(callers[i].GetElementType(), callers[i]);
            }
        }
    }
}
