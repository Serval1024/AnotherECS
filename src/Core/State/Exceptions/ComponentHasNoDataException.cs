using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ComponentHasNoDataException : Exception
    {
        public ComponentHasNoDataException(Type type)
            : base($"{DebugConst.TAG}Component has no data: '{type.Name}'.")
        { }
    }
}
