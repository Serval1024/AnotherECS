using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class FilterHasNoConditionException : Exception
    {
        public FilterHasNoConditionException(string name)
            : base($"{DebugConst.TAG}Filter has no filter condition: {name}.")
        { }
    }
}