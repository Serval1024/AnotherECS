using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{

    internal unsafe struct HistoryAllocatorProvider : IAllocatorProvider<HAllocator, HAllocator>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetStage0(Dependencies* dependencies)
            => &dependencies->stage0HAllocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HAllocator* GetStage1(Dependencies* dependencies)
            => &dependencies->stage1HAllocator;
    }
}