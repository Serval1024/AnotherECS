namespace AnotherECS.Serializer
{
    public interface ISerializeConstructor : ISerialize
    { }

    public interface ISerialize
    {
        void Pack(ref WriterContextSerializer writer);
        void Unpack(ref ReaderContextSerializer reader);
    }
}
