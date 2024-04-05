using AnotherECS.Core;
using AnotherECS.Debug;
using System;

namespace AnotherECS.Unity.Jobs.Exceptions
{
    public class ComponentNotSupportVersionException : Exception
    {
        public ComponentNotSupportVersionException(Type type)
            : base($"{DebugConst.TAG}Component does not contain version data: '{type.Name}'. Perhaps you forgot the '{nameof(IVersion)}' modifier.")
        { }
    }
}
