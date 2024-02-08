namespace AnotherECS.Core
{
    internal static class RepairMemoryUtils
    {
        private const uint ALLOCATOR_COUNT = 4;
        private static IRepairMemory[] _repairsTemp = new IRepairMemory[ALLOCATOR_COUNT];

        public unsafe static RepairMemoryContext Create(BAllocator* bAllocator, HAllocator* hAllocatorStage0, HAllocator* hAllocatorStage1)
        {
            _repairsTemp[bAllocator->GetId()] = bAllocator->GetRepairMemory();
            _repairsTemp[hAllocatorStage0->GetId()] = hAllocatorStage0->GetRepairMemory();
            _repairsTemp[hAllocatorStage1->GetId()] = hAllocatorStage1->GetRepairMemory();

            return new RepairMemoryContext(_repairsTemp);
        }
    }
}
