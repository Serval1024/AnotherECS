using System;

namespace AnotherECS.Serializer
{
    [Serializable]
    internal class UnknowTypeException : Exception
    {
        public UnknowTypeException(Type type, Exception innerException) 
            : base($"Type '{type}' not registered in serialization module.", innerException)
        {
        }

        public UnknowTypeException(uint typeId, Exception innerException)
            : base($"Type with id '{typeId}' not registered in serialization module.", innerException)
        {
        }
    }
}