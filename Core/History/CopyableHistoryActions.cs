﻿using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class CopyableHistoryActions<T>
        where T : unmanaged, ICopyable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDense(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, uint offset, ref T data)
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickOffsetData<T>>(layout.history.denseIndex++);
            if (element.tick != 0)
            {
                element.value.OnRecycle();
            }

            element.tick = tick;
            element.offset = offset;
            element.value.CopyFrom(in data);

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<T>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushFullDense(ref UnmanagedLayout<T> layout, uint tick, uint recordLength, ref ArrayPtr data)
        {
            ref var element = ref layout.history.denseBuffer.GetRef<TickDataPtr<T>>(layout.history.denseIndex++);
            if (element.tick != 0)
            {
                CallRecycle(ref element.value);
            }

            element.tick = tick;
            CopyFrom(ref element.value, ref data, layout.storage.denseIndex);

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<T>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        private unsafe static void CopyFrom(ref ArrayPtr destination, ref ArrayPtr source, uint denseIndex)
        {
            if (!destination.IsValide || destination.ElementCount != source.ElementCount)
            {
                destination.Resize<T>(source.ElementCount);
            }
            
            var sourcePtr = source.GetPtr<T>();
            var destinationPtr = destination.GetPtr<T>();

            for (uint i = 0; i < denseIndex; i++)
            {
                destinationPtr[i].CopyFrom(sourcePtr[i]);
            }
        }

        private unsafe static void CallRecycle(ref ArrayPtr data)
        {
            var dense = data.GetPtr<T>();
            for(int i = 0; i < data.ElementCount; i++)
            {
                dense[i].OnRecycle();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void HistoryClear(ref UnmanagedLayout<T> layout)
        {
            var denseBufferPtr = layout.history.denseBuffer.GetPtr<TickOffsetData<T>>();
            var count = layout.history.denseBuffer.ElementCount;

            for (int i = 0; i < count; ++i)
            {
                if (denseBufferPtr[i].tick != 0)
                {
                    denseBufferPtr[i].value.OnRecycle();
                }
            }

            HistoryActions<T>.HistoryClear(ref layout);
        }
    }
}