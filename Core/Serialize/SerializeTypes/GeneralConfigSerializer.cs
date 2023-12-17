using System;
using AnotherECS.Core;

namespace AnotherECS.Serializer
{
    public struct WorldConfigSerializer : IElementSerializer
    {
        public Type Type => typeof(StateConfig);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var worldConfig = (StateConfig)@value;
            writer.WriteStruct(worldConfig.general);
            writer.WriteStruct(worldConfig.history);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new StateConfig()
            {
                general = reader.ReadStruct<GeneralConfig>(),
                history = reader.ReadStruct<HistoryConfig>(),
            };
    }

    public struct GeneralConfigSerializer : IElementSerializer
    {
        public Type Type => typeof(GeneralConfig);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var generalConfig = (GeneralConfig)@value;
            writer.Write(generalConfig.entityCapacity);
            writer.Write(generalConfig.componentCapacity);
            writer.Write(generalConfig.recycleCapacity);
            writer.Write(generalConfig.archetypeCapacity);
            writer.Write(generalConfig.chunkLimit);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new GeneralConfig()
            {
                entityCapacity = reader.ReadUInt32(),
                componentCapacity = reader.ReadUInt32(),
                recycleCapacity = reader.ReadUInt32(),
                archetypeCapacity = reader.ReadUInt32(),
                chunkLimit = reader.ReadUInt32(),
            };
    }

    public struct HistoryConfigSerializer : IElementSerializer
    {
        public Type Type => typeof(HistoryConfig);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var historyConfig = (HistoryConfig)@value;
            writer.Write(historyConfig.recordTickLength);
            writer.Write(historyConfig.buffersCapacity);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new HistoryConfig()
            {
                recordTickLength = reader.ReadUInt32(),
                buffersCapacity = reader.ReadUInt32(),
            };
    }
}
