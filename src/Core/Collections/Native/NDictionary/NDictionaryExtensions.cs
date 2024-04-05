using AnotherECS.Core.Allocators;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public static class NDictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TAllocator, TKey, TValue, THashProvider>(this ref NDictionary<TAllocator, TKey, TValue, THashProvider> ndictionary)
            where TAllocator : unmanaged, IAllocator
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged, IDisposable
            where THashProvider : struct, IHashProvider<TKey, uint>
        {
            DeepDispose<NDictionary<TAllocator, TKey, TValue, THashProvider>, TValue>(ref ndictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TNDictionary, TValue>(this ref TNDictionary ndictionary)
            where TNDictionary : struct, INDictionary<TValue>
            where TValue : unmanaged, IDisposable
        {
            foreach (var value in ndictionary.Values)
            {
                value.Dispose();
            }
            ndictionary.Dispose();
        }
    }
}