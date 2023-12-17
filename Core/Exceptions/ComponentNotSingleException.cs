using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentNotSingleException : Exception
    {
        public ComponentNotSingleException(Type type)
            : base($"{DebugConst.TAG}Component is not registered as {nameof(Core.ISingle)}: '{type.Name}'.")
        { }
    }
}
