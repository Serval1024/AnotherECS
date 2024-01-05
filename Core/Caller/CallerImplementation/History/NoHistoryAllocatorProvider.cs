using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NoHistoryAllocatorProvider : IAllocaterProvider<BAllocator, BAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* Get(GlobalDependencies* dependencies)
            => &dependencies->bAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* GetAlt(GlobalDependencies* dependencies)
            => &dependencies->bAllocator;
    }
}