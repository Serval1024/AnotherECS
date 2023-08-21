using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ViewNotFoundedException : Exception
    {
        public ViewNotFoundedException(string id)
            : base($"{DebugConst.TAG}View not founded: {id}.")
        { }
    }
}