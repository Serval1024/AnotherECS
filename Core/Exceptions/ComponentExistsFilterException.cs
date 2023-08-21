using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ComponentExistsFilterException : Exception
    {
        public ComponentExistsFilterException(int componentId)
            : base($"{DebugConst.TAG}Component already added to filter. Component id: {componentId}.")
        { }
   }
}
