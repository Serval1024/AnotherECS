using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Collection
{
    public struct RingBuffer<T> : IEnumerable<T>
        where T : struct
    {
        private readonly T[] _data;
        private int _count;
        private int _position;

        public RingBuffer(int capacity)
            : this(capacity, Array.Empty<T>()) { }

        public RingBuffer(int capacity, T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (capacity < 1)
            {
                throw new ArgumentException($"Zero capacity is not a valid value.'");
            }
            if (items.Length > capacity)
            {
                throw new ArgumentException($"Not enough capacity for '{nameof(items)}'");
            }

            _data = new T[capacity];

            Array.Copy(items, _data, items.Length);
            _count = items.Length;

            _position = items.Length;
        }

        public int Position => _position;
        public int Capacity => _data.Length;
        public bool IsEmpty => Count == 0;
        public int Count => _count;

        public T GetLast()
        {
            ThrowIfEmpty();
            var index = _position - 1;
            if (index < 0)
            {
                index = Count - 1;
            }
            return _data[index];
        }

        public T this[int index]
        {
            get
            {
                ThrowIfEmpty();
                if (index >= _count)
                {
                    throw new IndexOutOfRangeException($"Index '{index}'. Buffer size is '{_count}.'");
                }
                return _data[index];
            }
            set
            {
                ThrowIfEmpty();
                if (index >= _count)
                {
                    throw new IndexOutOfRangeException($"Index '{index}'. Buffer size is '{_count}.'");
                }
                _data[index] = value;
            }
        }

        public void Push(T item)
        {
            _data[_position] = item;
            if (++_position >= Capacity)
            {
                _position = 0;
            }
            if (_count < Capacity)
            {
                ++_count;
            }
        }

        public void Clear()
        {
            _count = 0;
            Array.Clear(_data, 0, _data.Length);
        }

        public T[] ToArray()
        {
            T[] result = new T[Count];
            int index = 0;
            foreach (var item in this)
            {
                result[index] = item;
                ++index;
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_position != 0)
            {
                foreach (var item in new ArraySegment<T>(_data, 0, _position).Reverse())
                {
                    yield return item;
                }
            }
            foreach (var item in new ArraySegment<T>(_data, _position, _count - _position).Reverse())
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void ThrowIfEmpty()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }
        }
    }
}