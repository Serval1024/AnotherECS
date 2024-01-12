using System;
using AnotherECS.Debug;

namespace AnotherECS.Exceptions
{
    public class HistoryTickLimitException : Exception
    {
        public HistoryTickLimitException(uint tick, uint revertTick, uint saveTickLength)
            : base($"{DebugConst.TAG}.It is impossible to rollback time to the specified tick '{revertTick}'. Current tick '{tick}'. RecordTickLength: '{saveTickLength}'")
        { }
    }
}
