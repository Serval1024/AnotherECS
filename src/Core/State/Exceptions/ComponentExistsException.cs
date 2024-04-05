using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ComponentExistsException : Exception
    {
        public ComponentExistsException(Type type)
            : base($"{DebugConst.TAG}Component already added to entity: '{type.Name}'.")
        { }

        public ComponentExistsException(ushort id)
            : base($"{DebugConst.TAG}Component already added to entity. Component id: '{id}'.")
        { }
    }
}
