using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public abstract class StorageHistory : History
    {
        private uint _subjectId;

        public uint SubjectId
            => _subjectId;

        internal StorageHistory(ref ReaderContextSerializer reader, TickProvider tickProvider) 
            : base(ref reader, tickProvider) { }


        public StorageHistory(in HistoryConfig config, TickProvider tickProvider, uint subjectId)
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