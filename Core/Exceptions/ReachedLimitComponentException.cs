using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReachedLimitComponentException : Exception
    {
        public ReachedLimitComponentException(int limit)
            : base($"{DebugConst.TAG}The limit of the maximum components, limit: '{limit}'.")
        { }
    }
}
