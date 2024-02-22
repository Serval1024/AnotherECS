using System;

namespace AnotherECS.Core.Exceptions
{
    internal class InjectException : Exception
    {
        public InjectException(Type type, string filedName)
            : base($"There is no suitable type for injection into the field. Type: '{type.Name}', field: '{filedName}'.")
        { }
    }
}