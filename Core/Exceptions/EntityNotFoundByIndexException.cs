using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class EntityNotFoundByIndexException : Exception
    {
        public EntityNotFoundByIndexException(int index)
            : base($"{DebugConst.TAG}Entity with index not found: {index}.")
        { }
    }
}
