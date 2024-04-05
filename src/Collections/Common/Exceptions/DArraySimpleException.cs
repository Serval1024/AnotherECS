using AnotherECS.Debug;
using System;

namespace AnotherECS.Collections.Exceptions
{
    public class DArraySimpleException : Exception
    {
        public DArraySimpleException(Type type)
            : base($"{DebugConst.TAG}Dynamic collection can storage only unmanaged and blittable type: '{type.Name}'.")
        { }
    }
}
