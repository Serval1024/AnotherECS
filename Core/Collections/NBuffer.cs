using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NBuffer<T> : IDisposable, ISerialize
        where T : unmanaged
    {
        private NArray<T> _data;
        private uint _count;

        public bool IsEmpty
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NBuffer(uint capacity)
        {
            _data = new NArray<T>(capacity);
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T element)
        {
            if (_count == _data.Length)
            {
                _data.Resize(_count);
            }

            _data.Set(_count++, element);
        }

        public T Pop()
        {
#if !ANOTHERECS_RELEASE
            if (_count == 0)
            {
                throw new InvalidOperationException();
            }
#endif
            return _data.Get(--_count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_count);
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _count = reader.ReadUInt32();
            _data.Unpack(ref reader);
        }
    }
}
