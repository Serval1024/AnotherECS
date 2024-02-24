using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class EntityCastException : Exception
    {
        public EntityCastException()
            : base($"{DebugConst.TAG}The entity or state is invalid.")
        { }
    }
}
