using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    /*
    internal struct ByTickHistoryFeature<TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>,
        IHistory<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>, NArray<TDense>>,
        IIterator<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>

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
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, ref GlobalDepencies depencies)
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
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable, TUintNextNumber>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
            where TUintNextNumber : struct, INumberProvier<TDenseIndex>
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryTickClear<TSparse, TDense, TDenseIndex, TCopyable>(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.PushFullDense<TSparse, TDense, TDenseIndex, TCopyable>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, layout.storage.dense);
        }

        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ForEach<AIterable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>
        {
            AIterable iterable = default;
            ref var denseBuffer = ref layout.history.denseBuffer;
            var denseBufferPtr = denseBuffer.GetPtr();

            for (uint i = 0; i < denseBuffer.Length; ++i)
            {
                ref var dense = ref denseBufferPtr[i];
                if (dense.tick != 0)
                {
                    ref var components = ref denseBufferPtr[i].value;
                    var componentsPtr = denseBufferPtr[i].value.GetPtr();

                    for (uint j = 0; j < components.Length; ++j)
                    {
                        iterable.Each(ref layout, ref depencies, ref componentsPtr[j]);
                    }
                }
            }
        }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, uint tick)
            {
                HistoryActions.RevertToValueBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
            }
        }
    }*/
}
