using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentNotFoundedException : Exception
    {
        public ComponentNotFoundedException(Type type)
            : base($"{DebugConst.TAG}Entity does not contain the requested component: {type.Name}.")
        { }

        public ComponentNotFoundedException(ushort componentId)
            : base($"{DebugConst.TAG}Entity does not contain the requested component. Component id: {componentId}.")
        { }
    }
}
