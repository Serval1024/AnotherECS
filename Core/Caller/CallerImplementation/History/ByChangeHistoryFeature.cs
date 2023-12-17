using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    /*
    internal struct ByChangeHistoryFeature<TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TOData<TDense>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TOData<TDense>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TOData<TDense>>,
        IHistory<TSparse, TDense, TDenseIndex, TOData<TDense>, TDense>,
        IIterator<TSparse, TDense, TDenseIndex, TOData<TDense>>

        where TSparse : unmanaged, IEquatable<TSparse>
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, ref GlobalDepencies depencies)
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
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable, TUintNextNumber>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
            where TUintNextNumber : struct, INumberProvier<TDenseIndex>
        {
            TUintNextNumber uintNextNumber = default;
            HistoryActions.PushDense<TSparse, TDense, TDenseIndex, TCopyable>
                (ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, uintNextNumber.ToNumber(offset), ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryChangeClear<TSparse, TDense, TDenseIndex, TCopyable>(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TOData<TDense>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TOData<TDense>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TOData<TDense>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ForEach<AIterable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TSparse, TDense, TDenseIndex, TOData<TDense>>
        {
            AIterable iterable = default;
            ref var denseBuffer = ref layout.history.denseBuffer;
            var denseBufferPtr = denseBuffer.GetPtr();

            for (uint i = 0; i < denseBuffer.Length; ++i)
            {
                ref var dense = ref denseBufferPtr[i];
                if (dense.tick != 0)
                {
                    iterable.Each(ref layout, ref depencies, ref dense.value);
                }
            }
        }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TOData<TDense>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TOData<TDense>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, uint tick)
            {
                HistoryActions.RevertToValueBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
            }
        }
    }*/
}
