using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    public unsafe struct CallerDirtyHandler
    {
        private DirtyHandler<HAllocator> _handler;

        public CallerDirtyHandler(in DirtyHandler<HAllocator> handler)
        {
            _handler = handler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty()
        {
            _handler.Dirty();
        }
    }
}