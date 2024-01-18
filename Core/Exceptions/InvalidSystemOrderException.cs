using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class InvalidSystemOrderException : Exception
    {
        public InvalidSystemOrderException(Type type, string message, Exception innerException = null)
            : base($"{DebugConst.TAG}the system '{type}' has order problems. Details: '{message}'.", innerException)
        { }
    }
}
