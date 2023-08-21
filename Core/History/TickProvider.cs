using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public class TickProvider : ISerialize
    {
        public uint Tick;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(Tick);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Tick = reader.ReadUInt32();
        }
    }
}

