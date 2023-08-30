using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentAlreadyAddedFilterBuilderException : Exception
    {
        public ComponentAlreadyAddedFilterBuilderException(int componentId)
            : base($"{DebugConst.TAG}Component already added to filter builder. Component id: '{componentId}'.")
        { }
    }
}
