using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class InvalidEntityException : Exception
    {
        public InvalidEntityException()
            : base($"{DebugConst.TAG}Try access to invalid entity.")
        { }
    }
}
