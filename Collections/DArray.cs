using AnotherECS.Core;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DArray<T> : IInject<DArrayCaller>, IEnumerable<T>, ISerialize
        where T : unmanaged
    {
        private DArrayCaller _bind;
        private uint _id;
        private int _length;
#if ANOTHERECS_DEBUG
        private int _version;
#endif
        internal DArray(DArrayCaller bind, ushort id, int length)
        {
            _bind = bind;
            _id = id;
            _length = length;
#if ANOTHERECS_DEBUG
            _version = 0;
            Validate();
#endif
        }

#if ANOTHERECS_DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<DArrayCaller>.Construct(DArrayCaller bind)
        {
            Validate();
            _bind = bind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        { 
            Deallocate();
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(DArrayStorage bind)
        {
            _bind = bind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct()
        {
            Deallocate();
        }
#endif
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValide()
#if ANOTHERECS_DEBUG
            => _id != 0 && _version == GetVersion();
#else
            => _id != 0;
#endif
        public void Allocate(int length)
        {
            if (!_bind.IsValide)
            {
                throw new Exceptions.MissInjectException(typeof(DArray<T>));
            }

            if (length >= 0)
            {
                Deallocate();
                _id = _bind.Add<T>(length);
                _length = length;
#if ANOTHERECS_DEBUG
                _version = GetVersion();
#endif
            }
            else
            {
                throw new ArgumentException($"Argument '{nameof(length)}', must be equals or greater 0.");
            }
        }

        public void Deallocate()
        {
            if (IsValide())
            {
                _bind.Remove(_id);
                _id = 0;
                _length = 0;
            }
        }

        public void Resize(int capacity)
        {
            if (_length != capacity)
            {
                if (IsValide())
                {
                    var newId = _bind.Add<T>(capacity);

                    _bind.Copy(_id, newId, Math.Min(_length, capacity));

                    _bind.Remove(_id);
                    _id = newId;
                    _length = capacity;
                }
                else
                {
                    Allocate(capacity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetVersion()
            => _bind.GetVersion(_id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read(int index)
#if ANOTHERECS_DEBUG
        {
            if (!IsValide())
            {
                throw new Exceptions.DArrayInvalideException(this.GetType());
            }
            return ref _bind.Read<T>(_id, index);
        }
#else
            => ref _bind.Read<T>(_id, index);
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int index)
#if ANOTHERECS_DEBUG
        {
            if (!IsValide())
            {
                throw new Exceptions.DArrayInvalideException(GetType());
            }
            ref var temp = ref _bind.Get<T>(_id, index);
            _version = GetVersion();
            return ref temp;
        }
#else
            => ref _bind.Get<T>(_id, index);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
        {
            Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, ref T value)
#if ANOTHERECS_DEBUG
        {
            if (!IsValide())
            {
                throw new Exceptions.DArrayInvalideException(GetType());
            }
            _bind.Set(_id, index, ref value);
            _version = GetVersion();
        }
#else
            => _bind.Set(_id, index, ref value);
#endif

        public int IndexOf(T item)
            => IndexOf(ref item, _length);
        
        public bool Contains(T item)
            => IndexOf(ref item, _length) != -1;

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, Length);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, Length);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, ref value);
        }

        public void Clear()
        {
            _bind.Clear(_id);
        }

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(ref this);

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_id);
            writer.Write(_length);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _id = reader.ReadUInt32();
            _length = reader.ReadInt32();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetRaw(int index, ref T value)
        {
            _bind.SetRaw(_id, index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetRaw(int index, T value)
        {
            _bind.SetRaw(_id, index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ApplyVersionRaw()
        {
#if ANOTHERECS_DEBUG
            _version = GetVersion();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe int IndexOf(ref T item, int count)
        {
#if ANOTHERECS_DEBUG
            if (!IsValide())
            {
                throw new Exceptions.DArrayInvalideException(this.GetType());
            }
#endif
            var array = (T*)ReadUnsafe();

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; ++i)
            {
                if (comparer.Equals(array[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void CopyTo(T[] array, int arrayIndex, int count)
        {
#if ANOTHERECS_DEBUG
            if (!IsValide())
            {
                throw new Exceptions.DArrayInvalideException(this.GetType());
            }
            if (count > Length)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(count)}':{count} must be less or equal than '{nameof(Length)}': {Length}");
            }
#endif
            if (array == null)
            {
                throw new NullReferenceException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(arrayIndex));
            }
            if (count <= 0)
            {
                throw new IndexOutOfRangeException(nameof(count));
            }
            if (array.Length - arrayIndex < count)
            {
                throw new ArgumentException($"There is not enough space in {nameof(array)} to copy.");
            }

            var data = (T*)ReadUnsafe();
            var iMax = Math.Min(array.Length - arrayIndex, count);
            for (int i = 0; i < iMax; ++i)
            {
                array[i + arrayIndex] = data[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IncVersion()
        {
            _bind.IncVersion(_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void* ReadUnsafe()
            => _bind.Read(_id);

        internal DArrayCaller Bind
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bind;
        }

        internal uint Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id;
        }

#if ANOTHERECS_DEBUG
        private void Validate()
        {
            if (!ComponentUtils.IsSimple(typeof(T)))
            {
                throw new Exceptions.DArraySimpleException(typeof(T));
            }
        }
#endif


        public struct Enumerator : IEnumerator<T>
        {
            private readonly DArray<T> _data;
            private int _current;
            private readonly int _length;
#if ANOTHERECS_DEBUG
            private readonly int _version;
#endif
            public Enumerator(ref DArray<T> data)
            {
                _data = data;
                _length = _data.Length;
                _current = -1;
#if ANOTHERECS_DEBUG
                _version = _data.GetVersion();
#endif
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if ANOTHERECS_DEBUG
                    if (_version != _data.GetVersion())
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
                => ++_current < _length;

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
