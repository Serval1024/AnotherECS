using System;

namespace AnotherECS.Serializer
{
    public interface SerializeToUInt : ITypeToUInt
    {
        (uint id, Type iSerializereTypes)[] GetISerializeres();
    }
}
