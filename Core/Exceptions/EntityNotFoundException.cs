using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(int id)
            : base($"{DebugConst.TAG}Entity with id not found: {id}.")
        { }
    }
}
