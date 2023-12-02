using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;

namespace AnotherECS.Core.Caller
{
    internal struct IncrementStorageFeature<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => LayoutActions.GetSpaceCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<THistory, TNumberProvider>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
            where TNumberProvider : struct, INumberProvier<TDenseIndex>
        {
            ref var denseIndex = ref layout.storage.denseIndex;
#if !ANOTHERECS_RELEASE
            LayoutActions.CheckDenseLimit<TSparse, TDense, TDenseIndex, TTickData, TSparse>(ref layout);
#endif
            THistory history = default;
            history.PushCount(ref layout, ref depencies, denseIndex);
            TNumberProvider nextNumber = default;
            return nextNumber.ConverToNumber(denseIndex++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense> { }
    }
}
