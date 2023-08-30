using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class FilterNoInitializedException : Exception
    {
        public FilterNoInitializedException(string name)
            : base($"{DebugConst.TAG}Filter no initialized: '{name}'.")
        { }
    }
}