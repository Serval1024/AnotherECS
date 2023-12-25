using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    public struct InjectContainer
    {
        public WPtr<HAllocator> HAllocator { get; private set; }

        public InjectContainer(WPtr<HAllocator> hAllocator)
        {
            HAllocator = hAllocator;
        }
    }
}

