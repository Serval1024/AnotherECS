using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{

    internal unsafe struct HistoryAllocatorProvider : IAllocaterProvider<HAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* Get(GlobalDepencies* depencies)
            => &depencies->hAllocator;
    }
}