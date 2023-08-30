namespace AnotherECS.Core
{
    internal struct EntitiesArgs
    {
        public uint entityCapacity;
        public uint recycledCapacity;
        public uint gcEntityCheckPerTick;
        public HistoryByChangeArgs history;

        public EntitiesArgs(in GeneralConfig generalConfig, TickProvider tickProvider)
        {
            entityCapacity = generalConfig.entityCapacity;
            recycledCapacity = generalConfig.recycledCapacity;
            gcEntityCheckPerTick = generalConfig.gcEntityCheckPerTick;
            history = new HistoryByChangeArgs(generalConfig.history, tickProvider);
        }
    }

    public struct HistoryArgs
    {
        public uint recordTickLength;
        public TickProvider tickProvider;

        public HistoryArgs(in HistoryByChangeArgs historyByChangeArgs)
        {
            recordTickLength = historyByChangeArgs.recordTickLength;
            tickProvider = historyByChangeArgs.tickProvider;
        }

        public HistoryArgs(in HistoryByTickArgs historyByTickArgs)
        {
            recordTickLength = historyByTickArgs.recordTickLength;
            tickProvider = historyByTickArgs.tickProvider;
        }
    }

    public struct HistoryByChangeArgs
    {
        public uint buffersAddRemoveCapacity;
        public uint buffersChangeCapacity;
        public uint recordTickLength;
        public TickProvider tickProvider;

        public HistoryByChangeArgs(in HistoryConfig historyConfig, TickProvider tickProvider)
        {
            buffersAddRemoveCapacity = historyConfig.buffersAddRemoveCapacity;
            buffersChangeCapacity = historyConfig.buffersChangeCapacity;
            recordTickLength = historyConfig.recordTickLength;
            this.tickProvider = tickProvider;
        }
    }

    public struct HistoryByTickArgs
    {
        public uint buffersAddRemoveCapacity;
        public uint buffersFullCopyCapacity;
        public uint byTickArrayExtraSize;
        public uint recordTickLength;
        public TickProvider tickProvider;

        public HistoryByTickArgs(in HistoryConfig historyConfig, TickProvider tickProvider)
        {
            buffersAddRemoveCapacity = historyConfig.buffersAddRemoveCapacity;
            buffersFullCopyCapacity = historyConfig.buffersFullCopyCapacity;
            byTickArrayExtraSize = historyConfig.byTickArrayExtraSize;
            recordTickLength = historyConfig.recordTickLength;
            this.tickProvider = tickProvider;
        }
    }

    public struct DArrayArgs
    {
        public uint dArrayCapacity;
        public uint dArrayBuffersCapacity;
        public HistoryByChangeArgs history;

        public DArrayArgs(in GeneralConfig generalConfig, TickProvider tickProvider)
        {
            dArrayCapacity = generalConfig.dArrayCapacity;
            dArrayBuffersCapacity = generalConfig.history.dArrayBuffersCapacity;
            history = new HistoryByChangeArgs(generalConfig.history, tickProvider);
        }
    }
}

