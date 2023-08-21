using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentCastException : Exception
    {
        public ComponentCastException(Type componentType, Type requirementType) 
            : base($"{DebugConst.TAG}Can't cast '{componentType.Name}' to type '{requirementType}'.")
        { }
    }
}
