using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal interface IFrameData
    {
        public uint Tick { get; }
    }

    internal struct SparseData<U> : IFrameData, ISerialize
           where U : unmanaged
    {
        public uint Tick
            => tick;

        public uint tick;
        public U sparseValue;
        public int sparseIndex;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.WriteStruct(sparseValue);
            writer.Write(sparseIndex);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            sparseValue = reader.ReadStruct<U>();
            sparseIndex = reader.ReadInt32();
        }
    }
    internal struct TickData<U> : IFrameData, ISerialize
            where U : unmanaged
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

    internal struct RecycledData : IFrameData, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public ushort recycled;
        public ushort recycledIndex;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Write(recycled);
            writer.Write(recycledIndex);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            recycled = reader.ReadUInt16();
            recycledIndex = reader.ReadUInt16();
        }
    }

    internal struct TickBoolData : IFrameData, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public bool value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Write(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            value = reader.ReadBoolean();
        }
    }

    internal enum Op : byte
    {
        ADD = 1 << 0,
        REMOVE = 1 << 1,
        BOTH = ADD | REMOVE,
    }
}
