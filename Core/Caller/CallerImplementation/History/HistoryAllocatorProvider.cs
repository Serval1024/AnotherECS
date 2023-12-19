using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{

    internal unsafe struct HistoryAllocatorProvider : IAllocaterProvider<HAllocator, HAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* Get(GlobalDepencies* depencies)
            => &depencies->hAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetAlt(GlobalDepencies* depencies)
            => &depencies->altHAllocator;
    }
}