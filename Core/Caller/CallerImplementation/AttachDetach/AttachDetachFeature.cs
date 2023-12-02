using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal struct AttachDetachFeature<TSparse> : IData, IAttachDetachProvider<TSparse>, IBoolConst
        where TSparse : unmanaged
    {
        public State state;
        public NArray<TSparse> bufferCopyTemp;
        public NArray<Op> opsTemp;

        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, ref GlobalDepencies depencies)
        {
            this.state = state;
            bufferCopyTemp.Allocate(depencies.config.general.entityCapacity);
            opsTemp.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            bufferCopyTemp.Dispose();
            opsTemp.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<TSparse> GetSparseTempBuffer()
            => bufferCopyTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<Op> GetOps()
            => opsTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => state;
    }
}
