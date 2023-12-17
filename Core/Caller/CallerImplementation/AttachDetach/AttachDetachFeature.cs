using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachDetachFeature<TSparse> : IData, IAttachDetachProvider<TSparse>, IBoolConst
        where TSparse : unmanaged
    {
        public State state;
        public NArray<BAllocator, TSparse> bufferCopyTemp;
        public NArray<BAllocator, Op> opsTemp;

        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, GlobalDepencies* depencies)
        {
            this.state = state;
            bufferCopyTemp.Allocate(&depencies->bAllocator, depencies->config.general.entityCapacity);
            opsTemp.Allocate(&depencies->bAllocator, depencies->config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            bufferCopyTemp.Dispose();
            opsTemp.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, TSparse> GetSparseTempBuffer()
            => bufferCopyTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, Op> GetOps()
            => opsTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => state;
    }
}
