using AnotherECS.Serializer;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public abstract class History : IHistory
    {
        protected uint _recordHistoryLength = 0;
        private readonly TickProvider _tickProvider;

        protected uint Tick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tickProvider.tick;
        }
        
        internal History(ref ReaderContextSerializer reader, TickProvider tickProvider)
        {
            _tickProvider = tickProvider;
            Unpack(ref reader);
        }
        
        public History(in HistoryArgs args)
        {
            _tickProvider = args.tickProvider;
            _recordHistoryLength = args.recordTickLength;
        }

        public virtual void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_recordHistoryLength);
        }

        public virtual void Unpack(ref ReaderContextSerializer reader)
        {
            _recordHistoryLength = reader.ReadUInt32();
        }
    }
}