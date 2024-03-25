using System.Collections.Generic;

namespace AnotherECS.Serializer
{
    public interface ISerializer
    {
        byte[] Pack(object data)
            => Pack(data, null);

        object Unpack(byte[] data)
            => Unpack(data, null);

        byte[] Pack(object data, IEnumerable<DependencySerializer> dependencies);
        object Unpack(byte[] data, IEnumerable<DependencySerializer> dependencies);
    }
}
