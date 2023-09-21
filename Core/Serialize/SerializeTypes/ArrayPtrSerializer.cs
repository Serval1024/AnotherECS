using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;
using System;

namespace AnotherECS.Serializer
{
    public unsafe struct ArrayPtrSerializer : IElementSerializer
    {
        private readonly CountMeta _сount;

        public Type Type => typeof(ArrayPtr);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var arrayPtr = (ArrayPtr)@value;
            _сount.Pack(ref writer, arrayPtr.ByteLength);
            _сount.Pack(ref writer, arrayPtr.ElementCount);
            writer.Write(arrayPtr.GetPtr(), arrayPtr.ByteLength);
        }


        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
        {
            uint length = _сount.Unpack(ref reader);
            uint count = _сount.Unpack(ref reader);
            var buffer = UnsafeMemory.Malloc(length);
            reader.Read(buffer, length);

            return new ArrayPtr(buffer, length, count);
        }
    }
}
