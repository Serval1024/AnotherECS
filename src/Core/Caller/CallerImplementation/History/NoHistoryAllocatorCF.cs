using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NoHistoryAllocatorCF : IAllocatorProvider<BAllocator, BAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* GetStage0(Dependencies* dependencies)
            => &dependencies->bAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BAllocator* GetStage1(Dependencies* dependencies)
            => &dependencies->bAllocator;
    }
}