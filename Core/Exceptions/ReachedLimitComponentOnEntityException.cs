using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ReachedLimitComponentOnEntityException : Exception
    {
        public ReachedLimitComponentOnEntityException(uint limit)
            : base($"{DebugConst.TAG}The limit of the maximum components on entity, limit: '{limit}'.")
        { }
    }
}
