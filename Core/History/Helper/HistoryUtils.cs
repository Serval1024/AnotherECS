using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Debug;

namespace AnotherECS.Core
{
    internal static class HistoryUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckAndResizeLoopBufferInternal<U>(ref uint index, ref ArrayPtr buffer, uint recordHistoryLength)
           where U : unmanaged, ITick
        {
            if (index == buffer.ElementCount)
            {
                if (buffer.GetRef<U>(buffer.ElementCount - 1).Tick - buffer.GetRef<U>(0).Tick < recordHistoryLength)
                {
                    buffer.Resize<U>(buffer.ElementCount << 1);
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
        public static void CheckAndResizeLoopBuffer<U>(ref uint index, ref ArrayPtr buffer, uint recordHistoryLength, string debugBufferName)
            where U : unmanaged, ITick
        {
#if ANOTHERECS_DEBUG
            var isResized = CheckAndResizeLoopBufferInternal<U>(ref index, ref buffer, recordHistoryLength);
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
