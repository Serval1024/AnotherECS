using AnotherECS.Core.Collection;
using System;

namespace AnotherECS.Core
{
    public unsafe struct InjectContainer : IDisposable  //Find by property name.
    {
        private readonly RawAllocator _rawAllocator;

        public WPtr<AllocatorSelector> BAllocator { get; private set; }
        public WPtr<AllocatorSelector> HAllocator { get; private set; }

        public InjectContainer(RawAllocator rawAllocator, BAllocator* bAllocator, HAllocator* hAllocator)
        {
            _rawAllocator = rawAllocator;
            AllocatorSelector* bAllocatorPointer = (AllocatorSelector*)_rawAllocator.Allocate<AllocatorSelector>().pointer;
            AllocatorSelector* hAllocatorPointer = (AllocatorSelector*)_rawAllocator.Allocate<AllocatorSelector>().pointer;

            *bAllocatorPointer = new AllocatorSelector(AllocatorType.BAllocator, bAllocator, hAllocator);
            *hAllocatorPointer = new AllocatorSelector(AllocatorType.HAllocator, bAllocator, hAllocator);

            BAllocator = new WPtr<AllocatorSelector>(bAllocatorPointer);
            HAllocator = new WPtr<AllocatorSelector>(hAllocatorPointer);
        }

        public void Dispose()
        {
            _rawAllocator.Deallocate(BAllocator.Value);
            _rawAllocator.Deallocate(HAllocator.Value);

            BAllocator = default;
            HAllocator = default;
        }
    }
}

