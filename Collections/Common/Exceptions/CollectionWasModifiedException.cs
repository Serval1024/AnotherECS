using AnotherECS.Debug;
using System;

namespace AnotherECS.Collections.Exceptions
{
    public class CollectionWasModifiedException : Exception
    {
        public CollectionWasModifiedException()
            : base($"{DebugConst.TAG}Collection was modified.")
        { }
    }
}
