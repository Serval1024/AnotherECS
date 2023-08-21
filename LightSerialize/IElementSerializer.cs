using System;

namespace AnotherECS.Serializer
{
    public interface IElementSerializer
    {
        public Type Type { get; }
        void Pack(ref WriterContextSerializer writer, object value);
        object Unpack(ref ReaderContextSerializer reader, object[] constructArgs);
    }
}
