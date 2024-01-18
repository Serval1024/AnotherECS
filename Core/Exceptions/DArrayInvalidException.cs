using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class DArrayInvalidException : Exception
    {
        public DArrayInvalidException(Type type)
            : base($"{DebugConst.TAG}DArray '{type.Name}' not valid. Perhaps you are forget call Allocate(). Or use multiple copy of DArray.")
        { }
    }
}
