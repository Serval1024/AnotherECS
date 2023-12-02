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
            writer.Write(generalConfig.recycledCapacity);
            writer.Write(generalConfig.componentCapacity);
            writer.Write(generalConfig.dArrayCapacity);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new GeneralConfig()
            {
                entityCapacity = reader.ReadUInt32(),
                recycledCapacity = reader.ReadUInt32(),
                componentCapacity = reader.ReadUInt32(),
                dArrayCapacity = reader.ReadUInt32(),
            };
    }

    public struct HistoryConfigSerializer : IElementSerializer
    {
        public Type Type => typeof(HistoryConfig);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var historyConfig = (HistoryConfig)@value;
            writer.Write(historyConfig.buffersChangeCapacity);
            writer.Write(historyConfig.buffersAddRemoveCapacity);
            writer.Write(historyConfig.dArrayBuffersCapacity);
            writer.Write(historyConfig.recordTickLength);
            writer.Write(historyConfig.byTickArrayExtraSize);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new HistoryConfig()
            {
                buffersChangeCapacity = reader.ReadUInt32(),
                buffersAddRemoveCapacity = reader.ReadUInt32(),
                dArrayBuffersCapacity = reader.ReadUInt32(),
                recordTickLength = reader.ReadUInt32(),
                byTickArrayExtraSize = reader.ReadUInt32(),
            };
    }
}
