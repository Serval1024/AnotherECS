using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal unsafe struct GlobalDepencies : ISerialize
    {
        public Entities entities;
        public Archetype archetype;

        public StateConfig config;
        public BAllocator bAllocator;
        public HAllocator hAllocator;
        public TickProvider tickProvider;
        public InjectContainer injectContainer;

        public Filters filters;
        public uint componentTypesCount;

        public MemoryRebinderContext currentMemoryRebinder;

        public void Pack(ref WriterContextSerializer writer)
        {
            bAllocator.Pack(ref writer);
            hAllocator.Pack(ref writer);
            
            entities.Pack(ref writer);
            archetype.Pack(ref writer);
            writer.WriteStruct(config);
            writer.WriteStruct(tickProvider);
            writer.Write(componentTypesCount);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            bAllocator.Unpack(ref reader);
            hAllocator.Unpack(ref reader);
            
            entities.Unpack(ref reader);
            archetype.Unpack(ref reader);
            config = reader.ReadStruct<StateConfig>();
            tickProvider = reader.ReadStruct<TickProvider>();
            componentTypesCount = reader.ReadUInt32();
        }
    }
}
