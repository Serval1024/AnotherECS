namespace AnotherECS.Serializer
{
    public struct CountMeta
    {
        private readonly UInt32Serializer _count;

        public void Pack(ref WriterContextSerializer writer, uint value)
            => _count.PackConcrete(ref writer, value);

        public uint Unpack(ref ReaderContextSerializer reader)
            => _count.UnpackConcrete(ref reader);
    }
}
