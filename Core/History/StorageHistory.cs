using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    /*
    public abstract class StorageHistory : History
    {
        
        internal StorageHistory(ref ReaderContextSerializer reader, TickProvider tickProvider) 
            : base(ref reader, tickProvider) { }


        public StorageHistory(in HistoryConfig args, TickProvider tickProvider)
            : base(new HistoryArgs(args))
        {
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
    */
}