using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class NullEntityException : Exception
    {
        public NullEntityException()
            : base($"{DebugConst.TAG}Try access to null entity.")
        { }
    }
}
