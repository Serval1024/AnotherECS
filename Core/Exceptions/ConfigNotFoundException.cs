using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class ConfigNotFoundException : Exception
    {
        public ConfigNotFoundException(Type type)
            : base($"{DebugConst.TAG}State does not contain the requested config: '{type.Name}'.")
        { }
    }
}
