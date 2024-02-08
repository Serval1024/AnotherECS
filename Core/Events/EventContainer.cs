using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal struct EventContainer : ITickEvent, ISerialize
    {
        public uint Tick { get; private set; }
        public IEvent Value { get; private set; }

        public EventContainer(uint tick, IEvent @event)
        {
            Tick = tick;
            Value = @event;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(Tick);
            writer.Pack(Value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Tick = reader.ReadUInt32();
            Value = reader.Unpack<IEvent>();
        }
    }
}

