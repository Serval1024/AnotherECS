using System;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal unsafe struct Dependencies : ISerialize, IDisposable
    {
        public Entities entities;
        public Archetype archetype;

        public StateConfig config;
        public BAllocator bAllocator;
        public HAllocator stage0HAllocator;
        public HAllocator stage1HAllocator;
        public TickProvider tickProvider;

        public InjectContainer injectContainer;
        public Filters filters;
        public uint componentTypesCount;

        public ushort stateId;

        public void Dispose()
        {
            injectContainer.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            bAllocator.Pack(ref writer);
            stage0HAllocator.Pack(ref writer);
            stage1HAllocator.Pack(ref writer);

            entities.Pack(ref writer);
            archetype.Pack(ref writer);
            writer.WriteStruct(config);
            writer.WriteStruct(tickProvider);
            writer.Write(componentTypesCount);
            writer.Write(stateId);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            bAllocator.Unpack(ref reader);
            stage0HAllocator.Unpack(ref reader);
            stage1HAllocator.Unpack(ref reader);

            entities.Unpack(ref reader);
            archetype.Unpack(ref reader);
            config = reader.ReadStruct<StateConfig>();
            tickProvider = reader.ReadStruct<TickProvider>();
            componentTypesCount = reader.ReadUInt32();
            stateId = reader.ReadUInt16();
        }
    }
}
