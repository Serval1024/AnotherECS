using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    public struct InjectContainer
    {
        public NPtr<HAllocator> HAllocator { get; private set; }

        public InjectContainer(NPtr<HAllocator> hAllocator)
        {
            HAllocator = hAllocator;
        }
    }
}

