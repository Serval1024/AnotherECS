using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReachedLimitEntityException : Exception
    {
        public ReachedLimitEntityException(int limit)
            : base($"{DebugConst.TAG}The limit of the maximum components for entity, limit: {limit}")
        { }
    }
}
