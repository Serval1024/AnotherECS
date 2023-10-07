using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static unsafe class MultiHistoryFacadeActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateForVersionDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.AllocateForVersionDense(ref layout, depencies.config.history.buffersAddRemoveCapacity, depencies.config.history.buffersChangeCapacity, depencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.AllocateDense(ref layout, depencies.config.history.buffersAddRemoveCapacity, depencies.config.history.buffersChangeCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateForFullDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.AllocateForFullDense(ref layout, depencies.config.history.buffersAddRemoveCapacity, depencies.config.history.buffersChangeCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateRecycle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.AllocateRecycle(ref layout, depencies.config.history.buffersAddRemoveCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSparse<USparse>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
            where USparse : unmanaged
        {
            HistoryActions<T>.AllocateSparse<USparse>(ref layout, depencies.config.history.buffersAddRemoveCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDenseSegment<USegment>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
            where USegment : unmanaged
        {
            HistoryActions<T>.AllocateDenseSegment<USegment>(ref layout, depencies.config.history.buffersAddRemoveCapacity, depencies.config.history.buffersChangeCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycledCount(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            PushRecycledCount(ref layout, ref depencies, layout.storage.recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var recycleIndex = layout.storage.recycleIndex;
            PushRecycle(ref layout, ref depencies, recycleIndex, layout.storage.recycle.GetPtr<uint>()[recycleIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCount(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            PushCount(ref layout, ref depencies, layout.storage.denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycledCount(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint recycleIndex)
        {
            HistoryActions<T>.PushRecycledCount(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint recycleIndex, uint recycle)
        {
            HistoryActions<T>.PushRecycle(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, recycleIndex, recycle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCount(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint count)
        {
            HistoryActions<T>.PushCount(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint offset, ref T data)
        {
            HistoryActions<T>.PushDense(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushFullDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.PushFullDense(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, ref layout.history.denseBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushVersionDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            HistoryActions<T>.PushVersionDense(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void PushSegment<USegment>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, USegment* data)
            where USegment : unmanaged
        {
            HistoryActions<T>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSparse<USparse>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint offset, USparse data)
            where USparse : unmanaged
        {
            HistoryActions<T>.PushSparse(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, offset, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycleCountBuffer(ref UnmanagedLayout<T> layout, uint tick)
        {
            HistoryActions<T>.RevertToSingleValueBuffer(tick, ref layout.storage.recycleIndex, ref layout.history.recycleCountBuffer, ref layout.history.recycleCountIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToRecycleBuffer(ref UnmanagedLayout<T> layout, uint tick)
        {
            HistoryActions<T>.RevertToMultiValueBuffer<uint>(tick, ref layout.storage.recycle, ref layout.history.recycleBuffer, ref layout.history.recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToCountBuffer(ref UnmanagedLayout<T> layout, uint tick)
        {
            HistoryActions<T>.RevertToSingleValueBuffer(tick, ref layout.storage.denseIndex, ref layout.history.countBuffer, ref layout.history.countIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToDenseBuffer(ref UnmanagedLayout<T> layout, uint tick)
        {
            HistoryActions<T>.RevertToMultiValueBuffer<uint>(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToFullDenseBuffer(ref UnmanagedLayout<T> layout, uint tick)
        {
            HistoryActions<T>.RevertToSingleValueBufferPtr<T>(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RevertToSparseBuffer<USparse>(ref UnmanagedLayout<T> layout, uint tick)
            where USparse : unmanaged
        {
            HistoryActions<T>.RevertToMultiValueBuffer<USparse>(tick, ref layout.storage.sparse, ref layout.history.sparseBuffer, ref layout.history.sparseIndex);
        }

        public static void RevertToSparseAttachDetachBuffer<USparse>(ref UnmanagedLayout<T> layout, uint tick, ref ArrayPtr<Op> ops, ref ArrayPtr bufferCopyTemp)
            where USparse : unmanaged, IEquatable<T>
        {
            HistoryActions<T>.RevertToSparseBufferMulti<USparse>(tick, ref layout.storage.sparse, ref layout.history.sparseBuffer, ref layout.history.sparseIndex, ref bufferCopyTemp, ref ops);
        }
    }
}