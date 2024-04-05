using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ReachedLimitComponentException : Exception
    {
        public ReachedLimitComponentException(uint limit)
            : base($"{DebugConst.TAG}The limit of the maximum components, limit: '{limit}'.")
        { }
    }
}
