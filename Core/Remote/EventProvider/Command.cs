using AnotherECS.Serializer;

namespace AnotherECS.Core.Remote
{
    public struct Command : ISerialize
    {
        public CommandType commandType;
        public object data;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(commandType);
            writer.Pack(data);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            reader.ReadEnum<CommandType>();
            data = reader.Unpack();
        }

        public enum CommandType : byte
        {
            Event = 0,
            State = 1,
        }
    }
}
