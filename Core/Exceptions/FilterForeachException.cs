using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class FilterForeachException : Exception
    {
        public FilterForeachException()
            : base($"{DebugConst.TAG}Filter enumerator is broken. �heck filter bypass loop.")
            { }
    }
}
