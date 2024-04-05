using AnotherECS.Debug;
using System;

namespace AnotherECS.Unity.Exceptions
{
    public class ViewNotFoundException : Exception
    {
        public ViewNotFoundException(string id)
            : base($"{DebugConst.TAG}View not found: '{id}'.")
        { }
    }
}