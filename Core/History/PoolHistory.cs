using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public abstract class PoolHistory : History
    {
        private uint _subjectId;

        public uint SubjectId
            => _subjectId;

        internal PoolHistory(ref ReaderContextSerializer reader, TickProvider tickProvider) 
            : base(ref reader, tickProvider) { }


        public PoolHistory(in HistoryConfig config, TickProvider tickProvider, uint subjectId)
            : base(config, tickProvider)
        {
            _subjectId = subjectId;
        }

        public override void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_subjectId);
        }

        public override void Unpack(ref ReaderContextSerializer reader)
        {
            _subjectId = reader.ReadUInt32();
        }
    }
}