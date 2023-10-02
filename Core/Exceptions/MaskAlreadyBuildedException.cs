using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class MaskAlreadyBuildedException : Exception   //TODO SER REMOVE?
    {
        public MaskAlreadyBuildedException()
            : base($"{DebugConst.TAG}Mask already builded.")
        { }
    }
}
