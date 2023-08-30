using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentExistsExcludeException : Exception
    {
        public ComponentExistsExcludeException(Type type)
            : base($"{DebugConst.TAG}Component already added in exclude list: '{type.Name}'.")
        { }
    }
}


