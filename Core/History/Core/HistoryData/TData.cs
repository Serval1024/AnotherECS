using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public struct TData<TDense> : ITickData<TDense>, ISerialize
    {
        public uint Tick
            => tick;

        public TDense Value
            => value;

        public uint tick;
        public TDense value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.WriteStruct(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            value = reader.ReadStruct<TDense>();
        }
    }
}
