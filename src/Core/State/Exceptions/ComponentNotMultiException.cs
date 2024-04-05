using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ComponentNotMultiException : Exception
    {
        public ComponentNotMultiException(Type type)
            : base($"{DebugConst.TAG}Component is registered as {nameof(Core.ISingle)}: '{type.Name}'.")
        { }
    }
}
