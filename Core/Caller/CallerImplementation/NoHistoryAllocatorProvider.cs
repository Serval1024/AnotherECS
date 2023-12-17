using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NoHistoryAllocatorProvider : IAllocaterProvider<BAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* Get(GlobalDepencies* depencies)
            => &depencies->bAllocator;
    }
}