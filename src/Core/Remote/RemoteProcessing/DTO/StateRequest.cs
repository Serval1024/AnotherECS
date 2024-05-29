using AnotherECS.Serializer;

namespace AnotherECS.Core.Remote
{
    public struct StateRequest : ISerialize
    {
        internal uint MessageId;

        public long PlayerId;
        public SerializationLevel SerializationLevel;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(MessageId);
            writer.Write(PlayerId);
            writer.Write(SerializationLevel);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            MessageId = reader.ReadUInt32();
            PlayerId = reader.ReadInt64();
            SerializationLevel = reader.ReadEnum<SerializationLevel>();
        }
    }
}