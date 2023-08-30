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
        public uint sparseIndex;

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
            sparseIndex = reader.ReadUInt32();
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

    internal struct RecycledData<U> : IFrameData, ISerialize
        where U : unmanaged
    {
        public uint Tick
            => tick;

        public uint tick;
        public U recycled;
        public U recycledIndex;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.WriteStruct(recycled);
            writer.WriteStruct(recycledIndex);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            recycled = reader.ReadStruct<U>();
            recycledIndex = reader.ReadStruct<U>();
        }
    }

    internal struct ElementOffsetData<U> : IFrameData, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public uint offset;
        public U data;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.Write(offset);
            writer.WriteStruct(data);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            offset = reader.ReadUInt32();
            data = reader.ReadStruct<U>();
        }
    }

    internal struct ComponentData<T> : IFrameData, ISerialize
    {
        public uint Tick
            => tick;

        public uint tick;
        public T component;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
            writer.WriteStruct(component);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
            component = reader.ReadStruct<T>();
        }
    }

    internal enum Op : byte
    {
        ADD = 1 << 0,
        REMOVE = 1 << 1,
        BOTH = ADD | REMOVE,
    }
}
