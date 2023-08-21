namespace AnotherECS.Serializer
{
    public struct ArrayUnmanagedMeta
    {
        private readonly CountMeta _сount;

        public unsafe void Write<T>(ref WriterContextSerializer writer, T[] data, int count)
        where T : unmanaged
        {
            if (count > data.Length)
            {
                count = data.Length;
            }

            _сount.Pack(ref writer, (uint)data.Length);
            _сount.Pack(ref writer, (uint)count);

            fixed (T* ptr = data)
            {
                byte* ptrByte = (byte*)ptr;
                for (int i = 0, iMax = count * sizeof(T); i < iMax; ++i)
                {
                    writer.Write(ptrByte[i]);
                }
            }
        }

        public unsafe T[] Read<T>(ref ReaderContextSerializer reader)
            where T : unmanaged
        {
            var length = _сount.Unpack(ref reader);
            var count = (int)_сount.Unpack(ref reader);
             
            var data = new T[length];

            fixed (T* ptr = data)
            {
                byte* ptrByte = (byte*)ptr;
                for (int i = 0, iMax = count * sizeof(T); i < iMax; ++i)
                {
                    ptrByte[i] = reader.ReadByte();
                }
            }

            return data;
        }
    }
}
