using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public struct NArrayMeta
    {
        private readonly UInt32Serializer _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack<TNArray>(ref WriterContextSerializer writer, ref INArray arrayPtr)
            where TNArray : unmanaged, INArray
        {
            if (arrayPtr.IsValide)
            {
                _count.PackConcrete(ref writer, arrayPtr.Length);
            }
            else
            {
                _count.PackConcrete(ref writer, uint.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack<TNArray>(ref WriterContextSerializer writer, ref TNArray arrayPtr)
            where TNArray : unmanaged, INArray
        {
            if (arrayPtr.IsValide)
            {
                _count.PackConcrete(ref writer, arrayPtr.Length);
            }
            else
            {
                _count.PackConcrete(ref writer, uint.MaxValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Unpack(ref ReaderContextSerializer reader)
        {
            return _count.UnpackConcrete(ref reader);
        }
    }
}
