using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class FilterPatternException : Exception
    {
        public FilterPatternException(int resultLength)
            : base(message: $"{DebugConst.TAG}The limit on the count of archetypes has been reached. Limit: '{resultLength}'")
        { }
    }
}
