namespace AnotherECS.Serializer
{
    public class DefaultSerializer : ISerializer
    {
        private readonly LightSerializer _impl;

        public DefaultSerializer()
        {
            _impl = new LightSerializer(new SerializeToUIntConverter(LightSerializer.START_CUSTOM_RANGE_CODES));
        }

        public byte[] Pack(object data)
            => _impl.Pack(data);

        public object Unpack(byte[] data)
            => _impl.Unpack(data);

        public byte[] PackCompress(object data)
            => CompressUtils.Compress(Pack(data));

        public object UnpackCompress(byte[] data)
            => _impl.Unpack(CompressUtils.Decompress(data));

        public T Unpack<T>(byte[] data)
            => (T)Unpack(data);

        public T UnpackCompress<T>(byte[] data)
            => (T)UnpackCompress(data);
    }
}
