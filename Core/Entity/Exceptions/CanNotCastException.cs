using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class CanNotCastException : Exception
    {
        public CanNotCastException()
            : base($"{DebugConst.TAG}The entity or state is invalid.")
        { }
    }
}
