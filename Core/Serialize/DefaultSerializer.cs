namespace AnotherECS.Serializer
{
    public class DefaultSerializer : ISerializer
    {
        private readonly ISerializer _serializerImpl;

        public DefaultSerializer()
        {
            _serializerImpl = new LightSerializer(new SerializeToUIntConverter(LightSerializer.START_CUSTOM_RANGE_CODES));
        }

        public byte[] Pack(object data)
            => _serializerImpl.Pack(data);

        public object Unpack(byte[] data)
            => _serializerImpl.Unpack(data);

        public T Unpack<T>(byte[] data)
            => (T)Unpack(data);
    }
}
