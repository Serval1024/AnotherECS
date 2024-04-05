using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class MaskIsEmptyException : Exception
    {
        public MaskIsEmptyException()
            : base($"{DebugConst.TAG}Mask have not any include components.")
        { }
    }
}
