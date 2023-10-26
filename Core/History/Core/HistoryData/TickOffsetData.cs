using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public struct TickOffsetData<TDense> : ITickData<TDense>, ISerialize
    {
        public uint Tick
            => tick;

        public TDense Value
            => value;

        public uint tick;
        public uint offset;
        public TDense value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Write(offset);
            writer.WriteStruct(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            offset = reader.ReadUInt32();
            value = reader.ReadStruct<TDense>();
        }
    }
}
