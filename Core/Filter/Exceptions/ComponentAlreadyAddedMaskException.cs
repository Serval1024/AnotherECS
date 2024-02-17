using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ComponentAlreadyAddedMaskException : Exception
    {
        public ComponentAlreadyAddedMaskException(uint componentId)
            : base($"{DebugConst.TAG}Component already added to mask. Component id: '{componentId}'.")
        { }
    }
}
