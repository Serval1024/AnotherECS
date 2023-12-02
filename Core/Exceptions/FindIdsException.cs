using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class FindIdsException : Exception
    {
        public FindIdsException(uint resultLength)
            : base(message: $"{DebugConst.TAG}The limit on the count of ids has been reached. Limit: '{resultLength}'")
        { }
    }
}
