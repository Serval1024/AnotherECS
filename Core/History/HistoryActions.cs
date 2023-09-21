using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal static class HistoryActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDense(ref UnmanagedLayout<T> layout, uint buffersAddRemoveCapacity, uint buffersChangeCapacity)
        {
            ref var history = ref layout.history;
            
            history.countBuffer = ArrayPtr.Create<TickData<uint>>(buffersAddRemoveCapacity);
            history.denseBuffer = ArrayPtr.Create<TickOffsetData<T>>(buffersChangeCapacity);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Inject(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var history = ref layout.history;
            var denseBuffer = history.denseBuffer.GetPtr<TickOffsetData<T>>();
            var denseLength = history.denseBuffer.ElementCount;

            ref var construct = ref layout.componentFunction.construct;

            for (int i = 0; i < denseLength; ++i)
            {
                construct(ref depencies, ref denseBuffer[i].value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRecycledCount(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint recycleIndex)
        {
            ref var element = ref layout.history.recycleCountBuffer.GetRef<TickData<uint>>(layout.history.recycleCountIndex++);
            element.tick = tick;
            element.value = recycleIndex;

            HistoryUtils.CheckAndResizeLoopBuffer<TickData<uint>>(ref layout.history.recycleCountIndex, ref layout.history.recycleCountBuffer, recordLength, nameof(layout.history.recycleCountBuffer));
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
        public static void PushCount(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint count)
        {
            ref var element = ref layout.history.countBuffer.GetRef<TickData<uint>>(layout.history.countIndex++);
            element.tick = tick;
            element.value = count;

            HistoryUtils.CheckAndResizeLoopBuffer<TickData<uint>>(ref layout.history.countIndex, ref layout.history.countBuffer, recordLength, nameof(layout.history.countBuffer));
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
        public static void PushFullDense(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, ref ArrayPtr data)
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickDataPtr<T>>(layout.history.denseIndex++);

            element.tick = tick;
            element.value.CreateFrom(data);

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<T>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void PushSegment<USegment>(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, USegment* data)
            where USegment : unmanaged
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickOffsetData<USegment>>(layout.history.denseIndex++);
            element.tick = tick;
            element.offset = (uint)(data - layout.storage.dense.GetPtr<USegment>());
            element.value = *data;

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<USegment>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSparse<USparse>(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint offset, USparse data)
            where USparse : unmanaged
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickOffsetData<USparse>>(layout.history.sparseIndex++);
            element.tick = tick;
            element.offset = offset;
            element.value = data;

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<USparse>>(ref layout.history.sparseIndex, ref layout.history.sparseBuffer, recordLength, nameof(layout.history.sparseBuffer));
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
        public static unsafe void RevertToSparseBufferMulti<USparse>(uint tick, ref ArrayPtr subject, ref ArrayPtr buffer, ref uint bufferIndex, ref ArrayPtr bufferCopy, ref ArrayPtr<Op> ops)
            where USparse : unmanaged, IEquatable<T>
        {
            var bufferPtr = buffer.GetPtr<TickOffsetData<USparse>>();
            var subjectPtr = subject.GetPtr<USparse>();

            bufferCopy.Resize(subject.ByteLength);
            bufferCopy.CopyFrom(subject);

            var bufferCopyPtr = bufferCopy.GetPtr<USparse>();
            var zero = default(USparse);

            if (ops.ElementCount != subject.ElementCount)
            {
                ops.Resize(subject.ElementCount);
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


            for (uint i = buffer.ElementCount - 1; i >= bufferIndex; --i)
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
                    bufferIndex = (i + 1) % buffer.ElementCount;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToMultiValueBuffer<TTickOffsetDataTypeData>(uint tick, ref ArrayPtr subject, ref ArrayPtr buffer, ref uint bufferIndex)
            where TTickOffsetDataTypeData : unmanaged
        {
            var bufferPtr = buffer.GetPtr<TickOffsetData<TTickOffsetDataTypeData>>();
            var subjectPtr = subject.GetPtr<TTickOffsetDataTypeData>();

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

            for (uint i = buffer.ElementCount - 1; i >= bufferIndex; --i)
            {
                var frame = bufferPtr[i];

                if (frame.tick > tick)
                {
                    subjectPtr[frame.offset] = frame.value;
                }
                else
                {
                    bufferIndex = (i + 1) % buffer.ElementCount;
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RevertToSingleValueBuffer<TTickDataTypeData>(uint tick, ref TTickDataTypeData subject, ref ArrayPtr buffer, ref uint bufferIndex)
            where TTickDataTypeData : unmanaged
        {
            var bufferPtr = buffer.GetPtr<TickData<TTickDataTypeData>>();

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
    }
}