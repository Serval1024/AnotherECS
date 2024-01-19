namespace AnotherECS.Serializer
{
    public struct TypeMeta
    {
        private readonly UInt32Serializer _id;

        public void Pack(ref WriterContextSerializer writer, uint value)
            => _id.PackConcrete(ref writer, value);

        public uint Unpack(ref ReaderContextSerializer reader)
            => _id.UnpackConcrete(ref reader);
    }
}
