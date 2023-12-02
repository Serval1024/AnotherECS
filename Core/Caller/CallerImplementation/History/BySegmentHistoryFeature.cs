using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct BySegmentHistoryFeature<TSparse, TDense, TDenseIndex, TSegment> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TOData<TSegment>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TOData<TSegment>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TOData<TSegment>>,
        ISegmentHistory<TSparse, TDense, TDenseIndex, TOData<TSegment>, TSegment>,
        IIterator<TSparse, TDense, TDenseIndex, TOData<TSegment>>

        where TSparse : unmanaged, IEquatable<TSparse>
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TSegment : unmanaged
    {
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies)
        {
            ref var history = ref layout.history;
            if (layout.storage.sparse.IsValide)
            {
                history.sparseBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            }
            history.countBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.denseBuffer.Allocate(depencies.config.history.buffersChangeCapacity);

            if (layout.storage.recycle.IsValide)
            {
                history.recycleCountBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
                history.recycleBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable, TUintNextNumber>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
            where TUintNextNumber : struct, INumberProvier<TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void PushDenseSegment
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies, TSegment* data)
        {
            HistoryActions.PushDenseSegment
                (ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void PushSegmentDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies, uint offset, uint index, TSegment* data)
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryChangeClear(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TOData<TSegment>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TOData<TSegment>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TOData<TSegment>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TSparse, TDense, TDenseIndex, TOData<TSegment>> { }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TOData<TSegment>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TOData<TSegment>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, uint tick)
            {
                HistoryActions.RevertToValueSegmentBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
            }
        }
    }
}
