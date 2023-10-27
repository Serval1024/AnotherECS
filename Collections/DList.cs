using AnotherECS.Core;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DList<T> : IInject<DArrayCaller>, IEnumerable<T>, IList<T>, ISerialize
        where T : unmanaged
    {
        private DArray<T> _data;
        private int _count;

#if !ANOTHERECS_RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<DArrayCaller>.Construct(DArrayCaller bind)
        { 
            InjectUtils.Contruct(ref _data, bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        {
            InjectUtils.Decontruct(ref _data);
        }
#else
        public void Construct(DArrayStorage bind)
            => _data.Construct(bind);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct()
            => _data.Deconstruct();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValide()
            => _data.IsValide();

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_count;
        }

        public bool IsReadOnly
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read(int index)
        {
            if (index >= _count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {_count}.");
            }
            return ref _data.Read(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int index)
        {
            if (index >= _count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {_count}.");
            }
            return ref _data.Get(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
            => Set(index, ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, ref T item)
        {
#if !ANOTHERECS_RELEASE
            FListHelper.ThrowIfOutOfRange(index, _count);
#endif
            _data.Set(index, ref item);
        }

        public void Add(T item)
        { 
            Add(ref item);
        }

        public void Add(ref T item)
        {
            if (_count == Capacity)
            {
                Resize((_count + 1) << 1);
            }

            _data.Set(_count++, ref item);
        }

        public void Insert(int index, T item)
        {
            Insert(index, ref item);
        }

        public void Insert(int index, ref T item)
        {
#if !ANOTHERECS_RELEASE
            FListHelper.ThrowIfOutOfRange(index, Capacity);
            if (!_data.IsValide())
            {
                throw new Exceptions.DArrayInvalideException(_data.GetType());
            }
#endif
            if (index == _count - 1)
            {
                Add(ref item);
            }
            else
            {
                if (_count == Capacity)
                {
                    Resize((_count + 1) << 1);
                }

                _data.Bind.MoveRigth(_data.Id, index, _count);
                _data.SetRaw(index, ref item);
#if !ANOTHERECS_RELEASE
                _data.ApplyVersionRaw();
#endif
                ++_count;
            }
        }

        public void RemoveAt(int index)
        {
#if !ANOTHERECS_RELEASE
            FListHelper.ThrowIfOutOfRange(index, Capacity);
            if (!_data.IsValide())
            {
                throw new Exceptions.DArrayInvalideException(_data.GetType());
            }
#endif
            if (index == _count - 1)
            {
                RemoveLast();
            }
            else
            {
                _data.Bind.MoveLeft(_data.Id, index, _count);
                _data.SetRaw(index + _count - 1, default);
#if !ANOTHERECS_RELEASE
                _data.ApplyVersionRaw();
#endif
            }
        }

        public unsafe int IndexOf(T item)
            => IndexOf(ref item);

        public unsafe int IndexOf(ref T item)
            => _data.IndexOf(ref item, _count);

        public bool Contains(T item)
            => Contains(ref item);

        public bool Contains(ref T item)
            => _data.IndexOf(ref item, _count) != -1;

        public bool Remove(T item)
            => Remove(ref item);

        public bool Remove(ref T item)
        {
            var index = _data.IndexOf(ref item, _count);
            if (index != -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex, _count);
        }

        public void RemoveLast()
        {
            if (_count != 0)
            {
                this[--_count] = default;
            }
        }

        public void Clear()
        {
            _data.Clear();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, ref value);
        }

        public void Resize(int capacity)
        {
            _data.Resize(capacity);
        }

        public IEnumerator<T> GetEnumerator()
          => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
            writer.Write(_count);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
            _count = reader.ReadInt32();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly DList<T> _data;
            private int _current;
            private readonly int _count;
#if !ANOTHERECS_RELEASE
            private readonly uint _version;
#endif
            public Enumerator(ref DList<T> data)
            {
                _data = data;
                _count = _data.Count;
                _current = -1;
#if !ANOTHERECS_RELEASE
                _version = _data._data._version;
#endif
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if !ANOTHERECS_RELEASE
                    if (_version != _data._data._version)
                    {
                        throw new InvalidOperationException("Collection was modified.");
                    }
#endif
                    return _data[_current];
                }
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_current < _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            { 
                _current = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
    }


}



