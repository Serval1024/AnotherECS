using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal interface ITickData<TDense>
    {
        public uint Tick { get; }
        public TDense Value { get; }
    }
    
    public struct TickData<TDense> : ITickData<TDense>, ISerialize
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

    public struct TickIndexerOffsetData<TDense> : ITickData<TDense>, ISerialize
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
    }

    public enum Op : byte
    {
        NONE = 0,
        ADD = 1 << 0,
        REMOVE = 1 << 1,
        BOTH = ADD | REMOVE,
    }
}
