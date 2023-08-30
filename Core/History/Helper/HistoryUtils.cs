using AnotherECS.Debug;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class HistoryUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckAndResizeLoopBufferInternal<U>(ref int index, ref U[] buffer, uint recordHistoryLength)
           where U : struct, IFrameData
        {
            if (index == buffer.Length)
            {
                if (buffer[^1].Tick - buffer[0].Tick < recordHistoryLength)
                {
                    Array.Resize(ref buffer, buffer.Length << 1);
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
        public static void CheckAndResizeLoopBuffer<U>(ref int index, ref U[] buffer, uint recordHistoryLength, string debugBufferName)
            where U : struct, IFrameData
        {
#if ANOTHERECS_DEBUG
            var isResized = CheckAndResizeLoopBufferInternal(ref index, ref buffer, recordHistoryLength);
            if (isResized)
            {
                Logger.HistoryBufferResized(debugBufferName, buffer.Length);
            }
#else
            CheckAndResizeLoopBufferInternal(ref index, ref buffer, recordHistoryLength);
#endif
        }
    }
}
