namespace AnotherECS.Serializer
{
    public interface ISerializer
    {
        byte[] Pack(object data);
        object Unpack(byte[] data);
    }
}
