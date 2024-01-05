using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{

    internal unsafe struct HistoryAllocatorProvider : IAllocaterProvider<HAllocator, HAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* Get(GlobalDependencies* dependencies)
            => &dependencies->hAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetAlt(GlobalDependencies* dependencies)
            => &dependencies->altHAllocator;
    }
}