using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NList<T> : IDisposable, ISerialize, IEnumerable<T>
        where T : unmanaged
    {
        internal NArray<T> _data;
        private uint _count;

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NList(NArray<T> array)
        {
            _data = array.ToNArray();
            _count = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NList(uint capacity)
        {
            _data = new NArray<T>(capacity);
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NList<T> CreateWrapper(NArray<T> other)
        {
            NList<T> wrapper = default;
            wrapper._data = other;
            wrapper._count = other.Length;
            return wrapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (_count == _data.Length)
            {
                _data.Resize(_count << 1);
            }
            _data.GetRef(_count++) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(ulong index)
            => _data.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(ulong index)
            => ref _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong index)
            => _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, T value)
            => _data.Set(index, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
            => _data.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
            => ref _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
            => _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
            => _data.Set(index, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(int index)
            => _data.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index)
            => ref _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
            => _data.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
            => _data.Set(index, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            if (_count != 0)
            {
                --_count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index)
        {
            RemoveAtInternal(index, _count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveAtInternal(uint index, uint capacity)
        {
            if (index == capacity - 1)
            {
                RemoveLast();
            }
            else
            {
                for (uint i = index; i < capacity - 1; ++i)
                {
                    _data.GetRef(i) = _data.GetRef(i + 1);
                }
                --_count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
            _count = 0;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
         => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public NArray<T> ToNArray()            
        {
            var result = new NArray<T>(_count);
            for (int i = 0; i < result.Length; ++i)
            {
                result.GetRef(i) = _data.GetRef(i);
            }

            return result;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NList<T> _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NList<T> data)
            {
                _data = data;
                _length = _data.Count;
                _current = uint.MaxValue;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.Get(_current);
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => unchecked(++_current < _length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
    }
}
