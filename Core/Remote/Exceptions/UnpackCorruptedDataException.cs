using System;

namespace AnotherECS.Core.Remote.Exceptions
{
    public class UnpackCorruptedDataException : Exception
    {
        public UnpackCorruptedDataException(Exception innerException)
            : base($"Fail to unpack data.", innerException)
        { }
    }
}