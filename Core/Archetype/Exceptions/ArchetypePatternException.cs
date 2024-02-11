using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ArchetypePatternException : Exception
    {
        public ArchetypePatternException(int resultLength)
            : base(message: $"{DebugConst.TAG}The limit on the count of archetypes has been reached. Limit: '{resultLength}'")
        { }
    }
}
