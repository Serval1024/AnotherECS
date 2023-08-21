using AnotherECS.Serializer;
using System.IO;

namespace AnotherECS.Core
{
    internal struct EventContainer : ITickEvent, ISerialize
    {
        public uint Tick { get; private set; }
        public BaseEvent Value { get; private set; }

        public EventContainer(uint tick, BaseEvent @event)
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
            Value = reader.Unpack<BaseEvent>();
        }
    }
}

