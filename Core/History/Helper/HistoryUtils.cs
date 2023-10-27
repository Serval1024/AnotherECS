using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Debug;

namespace AnotherECS.Core
{
    internal static class HistoryUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckAndResizeLoopBufferInternal<ETickDataDense, TDense>(ref uint index, ref ArrayPtr<ETickDataDense> buffer, uint recordHistoryLength)
            where ETickDataDense : unmanaged, ITickData<TDense>
            where TDense : unmanaged
        {
            if (index == buffer.ElementCount)
            {
                if (buffer.GetRef(buffer.ElementCount - 1).Tick - buffer.GetRef(0).Tick < recordHistoryLength)
                {
                    buffer.Resize(buffer.ElementCount << 1);
                    return true;
                }
                else
                {
                    index = 0;
                    return false;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckAndResizeLoopBuffer<ETickDataDense, TDense>(ref uint index, ref ArrayPtr<ETickDataDense> buffer, uint recordHistoryLength, string debugBufferName)
            where ETickDataDense : unmanaged, ITickData<TDense>
            where TDense : unmanaged
        {
#if !ANOTHERECS_RELEASE
            var isResized = CheckAndResizeLoopBufferInternal<ETickDataDense, TDense>(ref index, ref buffer, recordHistoryLength);
            if (isResized)
            {
                Logger.HistoryBufferResized(debugBufferName, buffer.ElementCount);
            }
#else
            CheckAndResizeLoopBufferInternal<U>(ref index, ref buffer, recordHistoryLength);
#endif
        }
    }
}
