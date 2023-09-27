using AnotherECS.Core;
using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Serializer
{
    public unsafe struct ArrayPtrSerializer : IElementSerializer
    {
        private static readonly CountMeta _сount;

        public Type Type => typeof(ArrayPtr);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var arrayPtr = (ArrayPtr)@value;
            Pack(ref writer, ref arrayPtr);
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => Unpack(ref reader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref ArrayPtr arrayPtr)
        {
            _сount.Pack(ref writer, arrayPtr.ByteLength);
            _сount.Pack(ref writer, arrayPtr.ElementCount);
            writer.Write(arrayPtr.GetPtr(), arrayPtr.ByteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr Unpack(ref ReaderContextSerializer reader)
        {
            uint length = _сount.Unpack(ref reader);
            uint count = _сount.Unpack(ref reader);
            var buffer = UnsafeMemory.Malloc(length);
            reader.Read(buffer, length);

            return new ArrayPtr(buffer, length, count);
        }
    }
}
