using System;
using AnotherECS.Debug;

namespace AnotherECS.Exceptions
{
    public class ComponentNotFoundException : Exception
    {
        public ComponentNotFoundException(Type type)
            : base($"{DebugConst.TAG}Entity does not contain the requested component: '{type.Name}'.")
        { }

        public ComponentNotFoundException(ushort componentId)
            : base($"{DebugConst.TAG}Entity does not contain the requested component. Component id: '{componentId}'.")
        { }
    }
}
