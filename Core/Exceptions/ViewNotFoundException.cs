using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ViewNotFoundException : Exception
    {
        public ViewNotFoundException(string id)
            : base($"{DebugConst.TAG}View not found: '{id}'.")
        { }
    }
}