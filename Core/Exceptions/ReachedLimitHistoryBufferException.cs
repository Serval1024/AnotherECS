using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReachedLimitHistoryBufferException : Exception
    {
        public ReachedLimitHistoryBufferException(int limit, int requestLimit)
            : base($"{DebugConst.TAG}The limit of the maximum ticks to storage, limit: {limit}. Request len of buffer: {requestLimit}.")
        { }
    }
}
