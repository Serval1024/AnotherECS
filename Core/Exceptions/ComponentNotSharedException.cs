using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentNotSharedException : Exception
    {
        public ComponentNotSharedException(Type type)
            : base($"{DebugConst.TAG}Component is not registered as {nameof(Core.IShared)}: '{type.Name}'.")
        { }
    }
}
