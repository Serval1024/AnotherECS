using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;

namespace AnotherECS.Core.Caller
{
    internal struct RecycleStorageFeature<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense> :
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
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.recycle.Allocate(depencies.config.general.recycledCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => LayoutActions.GetCount(ref layout, startIndex);

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
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;

            THistory history = default;
            if (recycleIndex > 0)
            {
                history.PushRecycledCount(ref layout, ref depencies, recycleIndex);
                return storage.recycle.Get(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
#if !ANOTHERECS_RELEASE
                LayoutActions.CheckDenseLimit<TSparse, TDense, TDenseIndex, TTickData, TSparse>(ref layout);
#endif
                history.PushCount(ref layout, ref depencies, denseIndex);
                TNumberProvider nextNumber = default;
                return nextNumber.ConverToNumber(denseIndex++);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        {
            LayoutActions.TryResizeDense(ref layout, layout.storage.recycle.Length << 1);
            ref var recycleIndex = ref layout.storage.recycleIndex;
            var recycle = layout.storage.recycle.GetPtr();

            THistory history = default;
            history.PushRecycled(ref layout, ref depencies, recycleIndex, recycle[recycleIndex]);
            history.PushRecycledCount(ref layout, ref depencies, recycleIndex);

            recycle[recycleIndex++] = id;
        }
    }
}
