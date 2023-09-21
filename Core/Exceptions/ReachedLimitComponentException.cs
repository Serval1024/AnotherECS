using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReachedLimitComponentException : Exception
    {
        public ReachedLimitComponentException(uint limit)
            : base($"{DebugConst.TAG}The limit of the maximum components, limit: '{limit}'.")
        { }
    }
}
