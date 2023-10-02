using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class MaskIsEmptyException : Exception   //TODO SER REMOVE?
    {
        public MaskIsEmptyException()
            : base($"{DebugConst.TAG}Mask have not any include or exclude components.")
        { }
    }
}
