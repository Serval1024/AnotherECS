using System;
using AnotherECS.Debug;

namespace AnotherECS.Exceptions
{
    public class ConfigExistsException : Exception
    {
        public ConfigExistsException(Type type)
            : base($"{DebugConst.TAG}Config already added to state: '{type.Name}'.")
        { }
    }
}
