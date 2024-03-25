using AnotherECS.Core.Allocators;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public static class NContainerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TAllocator, T>(this ref NContainer<TAllocator, T> ncontainer)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IDisposable
        {
            ncontainer.GetRef().Dispose();
            ncontainer.Dispose();
        }
    }
}