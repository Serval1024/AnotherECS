using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class DArrayInvalideException : Exception
    {
        public DArrayInvalideException(Type type)
            : base($"{DebugConst.TAG}DArray '{type.Name}' not invalide. Perhaps you are forget call Allocate(). Or use multiple copy of DArray.")
        { }
    }
}
