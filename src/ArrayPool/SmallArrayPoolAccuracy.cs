using System;

namespace AnotherECS.ArrayPool
{
    public struct SmallArrayPoolAccuracy<T>
    {
        private T[][] _data;

        public SmallArrayPoolAccuracy(int capacity)
        {
            _data = new T[capacity][];
            for (int i = 0; i < capacity; ++i)
            {
                _data[i] = new T[i];
            }
        }

        public T[] Empty()
            => Get(0);

        public T[] Get(int size)
        {
            if (size >= _data.Length)
            {
                var oldSize = _data.Length;
                Array.Resize(ref _data, size);

                for (int i = oldSize; i < _data.Length; ++i)
                {
                    _data[i] = new T[i];
                }
            }
            return _data[size];
        }
    }
}
