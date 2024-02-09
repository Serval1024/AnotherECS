using AnotherECS.Debug;
using System;

namespace AnotherECS.Collections.Exceptions
{
    public class DCollectionInvalidException : Exception
    {
        public DCollectionInvalidException(Type type)
            : base($"{DebugConst.TAG}Collection '{type.Name}' not valid. Perhaps you are forget call Allocate(). Or use multiple copy of DArray.")
        { }
    }
}
