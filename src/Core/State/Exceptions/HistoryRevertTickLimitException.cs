using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class HistoryRevertTickLimitException : Exception
    {
        public HistoryRevertTickLimitException(uint tick, uint revertTick, uint saveTickLength)
            : base($"{DebugConst.TAG}.It is impossible to rollback time to the specified tick '{revertTick}'. Current tick '{tick}'. RecordTickLength: '{saveTickLength}'")
        { }
    }
}
