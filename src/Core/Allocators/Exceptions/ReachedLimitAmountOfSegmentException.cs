using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Allocators.Exceptions
{
    public class ReachedLimitAmountOfSegmentException : Exception
    {
        public ReachedLimitAmountOfSegmentException(uint limit)
            : base($"{DebugConst.TAG}The limit of the maximum of segment per allocation, limit: '{limit}'.")
        { }
    }
}
