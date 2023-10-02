using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public struct ArrayPtrMeta
    {
        private readonly UInt32Serializer _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack<T>(ref WriterContextSerializer writer, ref ArrayPtr<T> arrayPtr)
            where T : unmanaged
        {
            _count.PackConcrete(ref writer, arrayPtr.ByteLength);
            _count.PackConcrete(ref writer, arrayPtr.ElementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref ArrayPtr arrayPtr)
        {
            _count.PackConcrete(ref writer, arrayPtr.ByteLength);
            _count.PackConcrete(ref writer, arrayPtr.ElementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, IArrayPtr arrayPtr)
        {
            _count.PackConcrete(ref writer, arrayPtr.ByteLength);
            _count.PackConcrete(ref writer, arrayPtr.ElementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (uint byteLength, uint elementCount) Unpack(ref ReaderContextSerializer reader)
            => (_count.UnpackConcrete(ref reader), _count.UnpackConcrete(ref reader));
    }
}
