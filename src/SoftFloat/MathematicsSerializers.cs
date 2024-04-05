using System;

namespace AnotherECS.Serializer
{
    public struct SFloatSerializer : IElementSerializer
    {
        public Type Type => typeof(sfloat);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write(((sfloat)value).RawValue);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => sfloat.FromRaw(reader.ReadUInt32());
    }

    public struct RandomSerializer : IElementSerializer
    {
        public Type Type => typeof(Mathematics.Random);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write(((Mathematics.Random)value).state);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => new Mathematics.Random(reader.ReadUInt32());
    }
}