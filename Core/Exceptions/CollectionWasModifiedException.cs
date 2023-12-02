using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class CollectionWasModifiedException : Exception
    {
        public CollectionWasModifiedException()
            : base($"{DebugConst.TAG}Collection was modified.")
        { }
    }
}
