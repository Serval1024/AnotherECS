using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public struct NArrayMeta
    {
        private readonly UInt32Serializer _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack<T>(ref WriterContextSerializer writer, ref NArray<T> arrayPtr)
            where T : unmanaged
        {
            if (arrayPtr.IsValide)
            {
                _count.PackConcrete(ref writer, arrayPtr.ByteLength);
                _count.PackConcrete(ref writer, arrayPtr.Length);
            }
            else
            {
                _count.PackConcrete(ref writer, uint.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref NArray arrayPtr)
        {
            if (arrayPtr.IsValide)
            {
                _count.PackConcrete(ref writer, arrayPtr.ByteLength);
                _count.PackConcrete(ref writer, arrayPtr.Length);
            }
            else
            {
                _count.PackConcrete(ref writer, uint.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, INArray arrayPtr)
        {
            if (arrayPtr.IsValide)
            {
                _count.PackConcrete(ref writer, arrayPtr.ByteLength);
                _count.PackConcrete(ref writer, arrayPtr.Length);
            }
            else
            {
                _count.PackConcrete(ref writer, uint.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (uint byteLength, uint elementCount) Unpack(ref ReaderContextSerializer reader)
        {
            var byteLength = _count.UnpackConcrete(ref reader);
            if (byteLength != uint.MaxValue)
            {
                return (byteLength, _count.UnpackConcrete(ref reader));
            }
            return (byteLength, 0);
        }
    }
}
