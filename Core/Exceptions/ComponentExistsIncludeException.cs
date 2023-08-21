using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentExistsIncludeException : Exception
    {
        public ComponentExistsIncludeException(Type type)
            : base($"{DebugConst.TAG}Component already added in include list: {type.Name}.")
        { }
    }
}