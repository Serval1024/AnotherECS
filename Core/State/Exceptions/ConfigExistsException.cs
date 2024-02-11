using AnotherECS.Debug;
using System;

namespace AnotherECS.Core.Exceptions
{
    public class ConfigExistsException : Exception
    {
        public ConfigExistsException(Type type)
            : base($"{DebugConst.TAG}Config already added to state: '{type.Name}'.")
        { }
    }
}
