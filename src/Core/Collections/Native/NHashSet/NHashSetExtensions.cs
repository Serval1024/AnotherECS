using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public static class NHashSetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TNHashSet, TValue>(this ref TNHashSet nhashset)
            where TNHashSet : struct, INHashSet<TValue>
            where TValue : unmanaged, IDisposable
        {
            foreach (var value in nhashset)
            {
                value.Dispose();
            }
            nhashset.Dispose();
        }
    }
}