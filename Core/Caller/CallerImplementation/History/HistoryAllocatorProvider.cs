using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{

    internal unsafe struct HistoryAllocatorProvider : IAllocatorProvider<HAllocator, HAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetStage0(GlobalDependencies* dependencies)
            => &dependencies->stage0HAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetStage1(GlobalDependencies* dependencies)
            => &dependencies->stage1HAllocator;
    }
}