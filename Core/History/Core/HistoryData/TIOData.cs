using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    /*
    public struct TIOData<TDense> : ILoopBuffer<TDense>, ISerialize
        where TDense : struct
    {
        public uint Tick
            => tick;

        public TDense Value
            => value;

        public uint tick;
        public uint index;
        public uint offset;
        public TDense value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Write(index);
            writer.Write(offset);
            writer.WriteStruct(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            index = reader.ReadUInt32();
            offset = reader.ReadUInt32();
            value = reader.ReadStruct<TDense>();
        }
    }*/
}
