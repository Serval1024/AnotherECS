using AnotherECS.Core.Collection;
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
            element.value = CreateFrom(ref data, layout.storage.denseIndex);

            HistoryUtils.CheckAndResizeLoopBuffer<TickOffsetData<T>>(ref layout.history.denseIndex, ref layout.history.denseBuffer, recordLength, nameof(layout.history.denseBuffer));
        }

        private unsafe static ArrayPtr CreateFrom(ref ArrayPtr data, uint denseIndex)
        {
            var dense = data.GetPtr<T>();
            var result = ArrayPtr.Create<T>(data.ElementCount);
            var destPtr = result.GetPtr<T>();

            for (uint i = 0; i < denseIndex; i++)
            {
                destPtr[i].CopyFrom(dense[i]);
            }
            return result;
        }

        private unsafe static void CallRecycle(ref ArrayPtr data)
        {
            var dense = data.GetPtr<T>();
            for(int i = 0; i < data.ElementCount; i++)
            {
                dense[i].OnRecycle();
            }
        }
    }
}