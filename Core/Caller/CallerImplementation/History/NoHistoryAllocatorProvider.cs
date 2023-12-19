using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NoHistoryAllocatorProvider : IAllocaterProvider<BAllocator, BAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* Get(GlobalDepencies* depencies)
            => &depencies->bAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* GetAlt(GlobalDepencies* depencies)
            => &depencies->bAllocator;
    }
}