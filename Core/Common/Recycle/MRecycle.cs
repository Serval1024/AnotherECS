using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal unsafe struct MRecycle
    {
        public int[] _data;
        public int _currentIndex;
        public int _counter;

        public MRecycle(int capacity)
        {
            _data = new int[capacity];
            _currentIndex = default;
            _counter = default;
            _counter = GetStartIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStartIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAllocated()
            => _counter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCount()
            => _counter - GetStartIndex() - _currentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Allocate()
        {
            if (_currentIndex > 0)
            {
                return _data[--_currentIndex];
            }
            else
            {
                return _counter++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Deallocate(int id)
        {
            if (_currentIndex == _data.Length)
            {
                Array.Resize(ref _data, _data.Length << 1);
            }

            _data[_currentIndex++] = id;
        }
    }
}