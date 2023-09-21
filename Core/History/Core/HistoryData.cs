using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal interface ITick
    {
        public uint Tick { get; }
    }

    internal unsafe struct TickDataPtr<U> : ITick, ISerialize
        where U : unmanaged
    {
        public uint Tick
            => tick;

        public uint tick;
        public ArrayPtr value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Pack(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            value = reader.Unpack<ArrayPtr>();
        }

        public void Dispose()
        {
            value.Dispose();
        }
    }

    internal struct TickData<U> : ITick, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public U value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.WriteStruct(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            value = reader.ReadStruct<U>();
        }
    }

    internal struct TickOffsetData<U> : ITick, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public uint offset;
        public U value;

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
            value = reader.ReadStruct<U>();
        }
    }

    internal enum Op : byte
    {
        NONE = 0,
        ADD = 1 << 0,
        REMOVE = 1 << 1,
        BOTH = ADD | REMOVE,
    }
}
