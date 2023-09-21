using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentNotMultiException : Exception
    {
        public ComponentNotMultiException(Type type)
            : base($"{DebugConst.TAG}Component is registered as {nameof(Core.IShared)}: '{type.Name}'.")
        { }
    }
}
