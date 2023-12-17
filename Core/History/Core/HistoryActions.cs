using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    /*
    internal static class HistoryActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCount<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick, uint recordLength, uint count)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
        {
            ref var element = ref layout.history.countBuffer.GetRef(layout.history.countIndex++);
            element.tick = tick;
            element.value = count;

            HistoryUtils.CheckAndResizeLoopBuffer<TData<uint>, uint>(ref layout.history.countIndex, ref layout.history.countBuffer, recordLength, nameof(layout.history.countBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycledCount<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick, uint recordLength, uint recycleIndex)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
        {
            ref var element = ref layout.history.recycleCountBuffer.GetRef(layout.history.recycleCountIndex++);
            element.tick = tick;
            element.value = recycleIndex;

            HistoryUtils.CheckAndResizeLoopBuffer<TData<uint>, uint>(ref layout.history.recycleCountIndex, ref layout.history.recycleCountBuffer, recordLength, nameof(layout.history.recycleCountBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void PushFullDense<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout, uint tick, uint recordLength, NArray<TDense> data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            ref var element = ref layout.history.denseBuffer.GetRef(layout.history.denseIndex++);

            TCopyable copyable = default;
            if (copyable.Is)
            {
                if (element.tick != 0)
                {
                    CallRecycle<TDense, TCopyable>(ref element.value);
                }

                element.tick = tick;
                CopyFrom<TDense, TCopyable>(ref element.value, ref layout.storage.dense, layout.storage.denseIndex);
            }
            else
            {
                element.tick = tick;
                element.value.CreateFrom(data);
            }

            HistoryUtils.CheckAndResizeLoopBuffer<TData<NArray<TDense>>, NArray<TDense>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void PushVersionDense<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TIOData<TDense>> layout, uint tick, uint recordLength)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            var denseBuffer = layout.history.denseBuffer;
            var denseBufferPtr = denseBuffer.GetPtr();
            var dense = layout.storage.dense;
            var densePtr = dense.GetPtr();

            var versionPtr = layout.storage.version.GetPtr();
            var versionIndexer = layout.history.versionIndexer.GetPtr();
            ref var denseIndex = ref layout.history.denseIndex;

            TCopyable copyable = default;
            if (copyable.Is)
            {
                for (uint i = 0, iMax = layout.storage.denseIndex; i < iMax; ++i)
                {
                    if (versionPtr[i] == tick)
                    {
                        ref var element = ref denseBufferPtr[denseIndex];
                        if (element.Tick != 0)
                        {
                            copyable.Recycle(ref element.value);
                        }

                        element.tick = tick;
                        element.offset = i;
                        copyable.CopyFrom(ref densePtr[i], ref element.value);
                        element.index = versionIndexer[i];
                        versionIndexer[i] = denseIndex;
                        ++denseIndex;
                    }
                }
            }
            else
            {
                for (uint i = 0, iMax = layout.storage.denseIndex; i < iMax; ++i)
                {
                    if (versionPtr[i] == tick)
                    {
                        ref var element = ref denseBufferPtr[denseIndex];

                        element.tick = tick;
                        element.offset = i;
                        densePtr[i] = element.value;
                        element.index = versionIndexer[i];
                        versionIndexer[i] = denseIndex;
                        ++denseIndex;
                    }
                }
            }
            HistoryUtils.CheckAndResizeLoopBuffer<TIOData<TDense>, TDense>(ref denseIndex, ref denseBuffer, recordLength, nameof(denseBufferPtr));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryChangeClear<TSparse, TDense, TDenseIndex, TSegment>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TSegment : unmanaged
        {
            layout.history.Clear();
            HistoryIndexesClear(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryChangeClear<TSparse, TDense, TDenseIndex, TSegment>
           (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TIOData<TSegment>> layout)
           where TSparse : unmanaged
           where TDense : unmanaged
           where TDenseIndex : unmanaged
           where TSegment : unmanaged
        {
            layout.history.Clear();
            HistoryIndexesClear(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryChangeClear<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            var denseBufferPtr = layout.history.denseBuffer.GetPtr();
            var count = layout.history.denseBuffer.Length;

            TCopyable copyable = default;
            if (copyable.Is)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (denseBufferPtr[i].Tick != 0)
                    {
                        copyable.Recycle(ref denseBufferPtr[i].value);
                    }
                }
            }
            layout.history.Clear();
            HistoryIndexesClear(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryTickClear<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TData<NArray<TDense>>> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            var denseBufferPtr = layout.history.denseBuffer.GetPtr();
            var count = layout.history.denseBuffer.Length;

            TCopyable copyable = default;
            if (copyable.Is)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (denseBufferPtr[i].Tick != 0)
                    {
                        var dense = denseBufferPtr[i].value;
                        CallRecycle<TDense, TCopyable>(ref dense);
                    }
                }
            }
            layout.history.Clear();
            HistoryIndexesClear(ref layout);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryVersionClear<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TIOData<TDense>> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            var denseBufferPtr = layout.history.denseBuffer.GetPtr();
            var count = layout.history.denseBuffer.Length;

            TCopyable copyable = default;
            if (copyable.Is)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (denseBufferPtr[i].Tick != 0)
                    {
                        copyable.Recycle(ref denseBufferPtr[i].value);
                    }
                }
            }
            layout.history.Clear();
            HistoryIndexesClear(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryIndexesClear<TSparse, TDense, TDenseIndex, TTickDataDense>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickDataDense> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickDataDense : unmanaged
        {
            layout.history.recycleCountIndex = 0;
            layout.history.recycleIndex = 0;
            layout.history.countIndex = 0;
            layout.history.denseIndex = 0;
            layout.history.sparseIndex = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void CopyFrom<TDense, TCopyable>
            (ref NArray<TDense> destination, ref NArray<TDense> source, uint denseIndex)
            where TDense : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>
        {
            if (!destination.IsValide || destination.Length != source.Length)
            {
                destination.Resize(source.Length);
            }

            TCopyable copyable = default;
            var sourcePtr = source.GetPtr();
            var destinationPtr = destination.GetPtr();

            for (uint i = 0; i < denseIndex; i++)
            {
                copyable.CopyFrom(ref sourcePtr[i], ref destinationPtr[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CallRecycle<TDense, TCopyable>
            (ref NArray<TDense> element)
            where TDense : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            TCopyable copyable = default;
            var dense = element;
            var densePtr = element.GetPtr();

            for (uint j = 0, jMax = dense.Length; j < jMax; j++)
            {
                copyable.Recycle(ref densePtr[j]);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void PushDenseSegment<TSparse, TDense, TDenseIndex, TSegment>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, uint tick, uint recordLength, TSegment* data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TSegment : unmanaged
        {
            ref var element = ref layout.history.denseBuffer.GetRef(layout.history.denseIndex++);
            element.tick = tick;
            element.offset = (uint)(data - (TSegment*)layout.storage.dense.GetPtr());
            element.value = *data;

            HistoryUtils.CheckAndResizeLoopBuffer<TOData<TSegment>, TSegment>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void PushDenseSegment<TSparse, TDense, TDenseIndex, TSegment>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TIOData<TSegment>> layout, uint tick, uint recordLength, uint offset, uint index, TSegment* data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TSegment : unmanaged
        {
            ref var element = ref layout.history.denseBuffer.GetRef(layout.history.denseIndex++);
            element.tick = tick;
            element.offset = offset;
            element.index = index;
            element.value = *data;

            HistoryUtils.CheckAndResizeLoopBuffer<TIOData<TSegment>, TSegment>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSparse<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick, uint recordLength, uint offset, TSparse data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
        {
            ref var element = ref layout.history.sparseBuffer.GetRef(layout.history.sparseIndex++);
            element.tick = tick;
            element.offset = offset;
            element.value = data;

            HistoryUtils.CheckAndResizeLoopBuffer<TOData<TSparse>, TSparse>(ref layout.history.sparseIndex, ref layout.history.sparseBuffer, recordLength, nameof(layout.history.sparseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDense<TSparse, TDense, TDenseIndex, TCopyable>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TDense>> layout, uint tick, uint recordLength, uint offset, ref TDense data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            ref var element = ref layout.history.denseBuffer.GetRef(layout.history.denseIndex++);

            TCopyable copyable = default;
            if (copyable.Is)
            {
                if (element.tick != 0)
                {
                    copyable.Recycle(ref element.value);
                }

                element.tick = tick;
                element.offset = offset;
                copyable.CopyFrom(ref data, ref element.value);
            }
            else
            {
                element.tick = tick;
                element.offset = offset;
                element.value = data;
            }

            HistoryUtils.CheckAndResizeLoopBuffer<TOData<TDense>, TDense>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSegmentDense<TSparse, TDense, TDenseIndex, TSegment>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TOData<TSegment>> layout, uint tick, uint recordLength, uint offset, ref TSegment data)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TSegment : unmanaged
        {
            ref var element = ref layout.history.denseBuffer.GetRef(layout.history.denseIndex++);

            element.tick = tick;
            element.offset = offset;
            element.value = data;

            HistoryUtils.CheckAndResizeLoopBuffer<TOData<TSegment>, TSegment>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycled<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick, uint recordLength, uint offset, TDenseIndex recycle)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
        {
            ref var element = ref layout.history.recycleBuffer.GetRef(layout.history.recycleIndex++);
            element.tick = tick;
            element.offset = offset;
            element.value = recycle;

            HistoryUtils.CheckAndResizeLoopBuffer<TOData<TDenseIndex>, TDenseIndex>(ref layout.history.recycleIndex, ref layout.history.recycleBuffer, recordLength, nameof(layout.history.recycleBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToSparseBuffer<TSparse>
            (uint tick, ref NArray<TSparse> subject, ref NArray<TOData<TSparse>> buffer, ref uint bufferIndex, ref NArray<TSparse> bufferCopy, ref NArray<Op> ops)
           where TSparse : unmanaged, IEquatable<TSparse>
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = subject.GetPtr();

            bufferCopy.Resize(subject.ByteLength);
            bufferCopy.CopyFrom(subject);

            var bufferCopyPtr = bufferCopy.GetPtr();
            TSparse zero = default;

            if (ops.Length != subject.Length)
            {
                ops.Resize(subject.Length);
            }
            else
            {
                ops.Clear();
            }

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        ref var copy = ref bufferCopyPtr[frame.offset];
                        ref var op = ref ops.GetRef(frame.offset);

                        if (op != Op.BOTH)
                        {
                            if (!copy.Equals(frame.value))
                            {
                                op |= Op.BOTH;
                            }
                            else if (copy.Equals(zero) && !frame.value.Equals(zero))
                            {
                                op |= Op.ADD;
                            }
                            else if (!copy.Equals(zero) && frame.value.Equals(zero))
                            {
                                op = Op.REMOVE;
                            }
                        }

                        subjectPtr[frame.offset] = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }


            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    ref var copy = ref bufferCopyPtr[frame.offset];
                    ref var op = ref ops.GetRef(frame.offset);

                    if (op != Op.BOTH)
                    {
                        if (!copy.Equals(frame.value))
                        {
                            op |= Op.BOTH;
                        }
                        else if (copy.Equals(zero) && !frame.value.Equals(zero))
                        {
                            op |= Op.ADD;
                        }
                        else if (!copy.Equals(zero) && frame.value.Equals(zero))
                        {
                            op = Op.REMOVE;
                        }
                    }

                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToValueBuffer<TData>
            (uint tick, ref NArray<TData> subject, ref NArray<TOData<TData>> buffer, ref uint bufferIndex)
            where TData : unmanaged
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = subject.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        subjectPtr[frame.offset] = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToValueSegmentBuffer<TData, TSegment>
            (uint tick, ref NArray<TData> subject, ref NArray<TOData<TSegment>> buffer, ref uint bufferIndex)
            where TData : unmanaged
            where TSegment : unmanaged
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = (TSegment*)subject.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        subjectPtr[frame.offset] = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToManualValueSegmentBuffer<TData, TSegment>
            (uint tick, ref NArray<TData> subject, ref NArray<TIOData<TSegment>> buffer, ref uint bufferIndex)
            where TData : unmanaged, IManualRevert<TSegment>
            where TSegment : unmanaged
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = subject.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        subjectPtr[frame.offset].OnRevert(frame.index, frame.value);
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    subjectPtr[frame.offset].OnRevert(frame.index, frame.value);
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToValueSegmentBuffer<TData, TSegment>
            (uint tick, ref NArray<TData> subject, ref NArray<TIOData<TSegment>> buffer, ref uint bufferIndex)
            where TData : unmanaged
            where TSegment : unmanaged
        {
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = (TSegment*)subject.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        subjectPtr[frame.offset] = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToValueBuffer<TData>
            (uint tick, ref TData subject, ref NArray<TData<TData>> buffer, ref uint bufferIndex)
            where TData : unmanaged
        {
            var bufferPtr = buffer.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    if (bufferPtr[i].tick <= tick)
                    {
                        var lastIndex = i + 1;
                        if (i < bufferIndex)
                        {
                            subject = bufferPtr[lastIndex].value;
                            bufferIndex = lastIndex;
                            return;
                        }
                        break;
                    }
                }
            }


            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                if (bufferPtr[i].tick <= tick)
                {
                    var lastIndex = i + 1;
                    if (lastIndex < buffer.Length)
                    {
                        subject = bufferPtr[lastIndex].value;
                        bufferIndex = lastIndex;
                        return;
                    }
                    break;
                }
            }

            subject = bufferPtr[bufferIndex].value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToValueIndexerBuffer<TData>
            (uint tick, ref NArray<TData> subject, ref NArray<TIOData<TData>> buffer, ref NArray<uint> indexerBuffer, ref uint bufferIndex)
           where TData : unmanaged
        {
            var indexerBufferPtr = indexerBuffer.GetPtr();
            var bufferPtr = buffer.GetPtr();
            var subjectPtr = subject.GetPtr();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick >= tick)
                    {
                        ref var earlyFrameIndex = ref frame.index;
                        if (earlyFrameIndex != 0)
                        {
                            subjectPtr[frame.offset] = bufferPtr[earlyFrameIndex].value;
                        }
                        indexerBufferPtr[frame.offset] = earlyFrameIndex;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.Length - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick >= tick)
                {
                    ref var earlyFrameIndex = ref frame.index;
                    if (earlyFrameIndex != 0)
                    {
                        subjectPtr[frame.offset] = bufferPtr[earlyFrameIndex].value;
                    }
                    indexerBufferPtr[frame.offset] = earlyFrameIndex;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.Length;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertTo<TSparse, TDense, TDenseIndex, TTickData, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse, KRevertDense>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TSparse : unmanaged, IEquatable<TSparse>
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged

            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse
            where KRevertDense : struct, IDenseRevert<TSparse, TDense, TDenseIndex, TTickData>
        {
            RevertToValueBuffer(tick, ref layout->storage.denseIndex, ref layout->history.countBuffer, ref layout->history.countIndex);

            RevertToValueBuffer(tick, ref layout->storage.recycleIndex, ref layout->history.recycleCountBuffer, ref layout->history.recycleCountIndex);
            RevertToValueBuffer(tick, ref layout->storage.recycle, ref layout->history.recycleBuffer, ref layout->history.recycleIndex);

            TIsUseSparse isUseSparse = default;
            if (isUseSparse.IsUseSparse)
            {
                if (attachDetachStorage.Is)
                {
                    var bufferCopy = attachDetachStorage.GetSparseTempBuffer();
                    var ops = attachDetachStorage.GetOps();
                    RevertToSparseBuffer(tick, ref layout->storage.sparse, ref layout->history.sparseBuffer, ref layout->history.sparseIndex, ref bufferCopy, ref ops);

                    TDetach detach = default;
                    if (detach.Is)
                    {
                        detach.Detach<JSparseBoolConst>(layout, attachDetachStorage.GetState(), ref ops);
                    }

                    KRevertDense revertDense = default;
                    revertDense.RevertDense(ref *layout, tick);


                    TAttach attach = default;
                    if (attach.Is)
                    {
                        attach.Attach<JSparseBoolConst>(layout, attachDetachStorage.GetState(), ref ops);
                    }
                }
                else
                {
                    RevertToValueBuffer(tick, ref layout->storage.sparse, ref layout->history.sparseBuffer, ref layout->history.sparseIndex);

                    KRevertDense revertDense = default;
                    revertDense.RevertDense(ref *layout, tick);
                }
            }

            TVersion version = default;
            if (version.Is)
            {
                LayoutActions.UpdateVersion(ref *layout, tick, layout->storage.denseIndex);
            }
        }

        

        
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateDense(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity, uint buffersChangeCapacity)
       {
           ref var history = ref layout.history;

           history.countBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
           history.denseBuffer = ArrayPtr.Create<TickOffsetData<T>>(buffersChangeCapacity);
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateForVersionDense(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity, uint buffersChangeCapacity, uint capacity)
       {
           ref var history = ref layout.history;

           history.countBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
           history.denseBuffer = ArrayPtr.Create<TickIndexerOffsetData<T>>(buffersChangeCapacity);
           history.versionIndexer = ArrayPtr.Create<uint>(capacity);
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateForFullDense(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity, uint buffersChangeCapacity)
       {
           ref var history = ref layout.history;

           history.countBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
           history.denseBuffer = ArrayPtr.Create<TickDataPtr<T>>(buffersChangeCapacity);
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateRecycle(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity)
       {
           ref var history = ref layout.history;

           history.recycleCountBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
           history.recycleBuffer = ArrayPtr.Create<TickOffsetData<uint>>(buffersAddRemoveCapacity);
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateSparse<USparse>(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity)
           where USparse : unmanaged
       {
           ref var history = ref layout.history;

           history.sparseBuffer = ArrayPtr.Create<TickOffsetData<USparse>>(buffersAddRemoveCapacity);
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static void AllocateDenseSegment<USegment>(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity, uint buffersChangeCapacity)
           where USegment : unmanaged
       {
           ref var history = ref layout.history;

           history.countBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
           history.denseBuffer = ArrayPtr.Create<TickOffsetData<USegment>>(buffersChangeCapacity);
       }

       public static void ResizeVersionIndexer(ref UnmanagedLayout<T> layout, uint capacity)
       {
           ref var history = ref layout.history;

           history.versionIndexer.Resize<uint>(capacity);
       }
       


        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycle(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint recycleIndex, uint recycle)
        {
            ref var element = ref layout.history.recycleBuffer.GetRef<TickOffsetData<uint>>(layout.history.recycleIndex++);
            element.tick = tick;
            element.offset = recycleIndex;
            element.value = recycle;

            HistoryUtils.CheckAndResizeLoopBuffer<TickData<uint>>(ref layout.history.recycleIndex, ref layout.history.recycleBuffer, recordLength, nameof(layout.history.recycleBuffer));
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDense(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint offset, ref T data)
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickOffsetData<T>>(layout.history.denseIndex++);
          
            element.tick = tick;
            element.offset = offset;
            element.value = data;

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<T>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }
        



        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToSingleSparseBuffer(uint tick, ref bool subject, ref ArrayPtr buffer, ref uint bufferIndex, out Op op)
        {
            var bufferPtr = buffer.GetPtr<TickOffsetData<bool>>();

            op = Op.NONE;

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    var frame = bufferPtr[i];

                    if (frame.tick > tick)
                    {
                        if (op != Op.BOTH)
                        {
                            if (subject != frame.value)
                            {
                                op |= Op.BOTH;
                            }
                            else if (!subject && frame.value)
                            {
                                op |= Op.ADD;
                            }
                            else if (subject && !frame.value)
                            {
                                op = Op.REMOVE;
                            }
                        }

                        subject = frame.value;
                    }
                    else
                    {
                        bufferIndex = i + 1;
                        return;
                    }
                }
            }

            for (uint i = buffer.ElementCount - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    if (op != Op.BOTH)
                    {
                        if (subject != frame.value)
                        {
                            op |= Op.BOTH;
                        }
                        else if (!subject && frame.value)
                        {
                            op |= Op.ADD;
                        }
                        else if (subject && !frame.value)
                        {
                            op = Op.REMOVE;
                        }
                    }

                    subject = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.ElementCount;
                    return;
                }
            }
        }
        






        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToSingleValueBufferPtr<TTickDataTypeData>(uint tick, ref ArrayPtr subject, ref ArrayPtr buffer, ref uint bufferIndex)
            where TTickDataTypeData : unmanaged
        {
            var bufferPtr = buffer.GetPtr<TickDataPtr<TTickDataTypeData>>();

            if (bufferIndex != 0)
            {
                for (uint i = bufferIndex - 1; i >= 0; --i)
                {
                    if (bufferPtr[i].tick <= tick)
                    {
                        var lastIndex = i + 1;
                        if (i < bufferIndex)
                        {
                            subject = bufferPtr[lastIndex].value;
                            bufferIndex = lastIndex;
                            return;
                        }
                        break;
                    }
                }
            }


            for (uint i = buffer.ElementCount - 1; i >= bufferIndex; --i)
            {
                if (bufferPtr[i].tick <= tick)
                {
                    var lastIndex = i + 1;
                    if (lastIndex < buffer.ElementCount)
                    {
                        subject = bufferPtr[lastIndex].value;
                        bufferIndex = lastIndex;
                        return;
                    }
                    break;
                }
            }

            subject = bufferPtr[bufferIndex].value;
        }
        


    }*/
}