using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct OptionalArrayPool<T>
    {
        public int Size => _size;

        private int _size;
        private readonly List<T[]> _insts;

        public OptionalArrayPool(int size)
        {
            _insts = new List<T[]>();
            _size = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int size)
        {
            if (size > _size)
            {
                _size = size;
                _insts.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Rent()
        {
            if (_insts.Count == 0)
            {
                return new T[_size];
            }
            else
            {
                var last = _insts.Count - 1;
                var inst = _insts[last];
                _insts.RemoveAt(last);
                return inst;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T[] array)
        {
            _insts.Add(array);
        }
    }
}