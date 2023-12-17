using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class EndOfStreamException : Exception
    {
        public EndOfStreamException()
            : base($"{DebugConst.TAG}The stream has reached end.")
        { }
    }
}
