using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Exceptions;
using AnotherECS.Serializer;

namespace AnotherECS.Collections
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [ForceBlittable]
    public struct DArray<T> : IInject<DArrayCaller>, IEnumerable<T>, ISerialize, ICArray
        where T : unmanaged
    {
        private DArrayCaller _bind;
        private uint _id;
        private int _length;

        internal DArray(DArrayCaller bind, uint id, int length)
        {
            _bind = bind;
            _id = id;
            _length = length;
#if !ANOTHERECS_RELEASE
            Validate();
#endif
        }

#if !ANOTHERECS_RELEASE
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
        public void Construct(DArrayCaller bind)
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

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id != 0;
        }

        public void Allocate(int length)
        {
            if (!_bind.IsValide)
            {
                throw new MissInjectException(typeof(DArray<T>));
            }

            if (length >= 0)
            {
                Deallocate();
                _id = _bind.Add<T>(length);
                _length = length;
            }
            else
            {
                throw new ArgumentException($"Argument '{nameof(length)}', must be equals or greater 0.");
            }
        }

        public void Deallocate()
        {
            if (IsValide)
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
                if (IsValide)
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
        public ref readonly T Read(int index)
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(this.GetType());
            }
            return ref _bind.Read<T>(_id, index);
        }
#else
            => ref _bind.Read<T>(_id, index);
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int index)
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(GetType());
            }
            return ref _bind.Get<T>(_id, index);
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
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(GetType());
            }
            _bind.Set(_id, index, ref value);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
            => new(GetUnsafe(), Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_id);
            writer.Write(_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _id = reader.ReadUInt32();
            _length = reader.ReadInt32();
        }

        object ICArray.Get(int index)
            => Get(index);

        void ICArray.Set(int index, object value)
        {
            Set(index, (T)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
            => new Enumerator(ref this);

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

#if !ANOTHERECS_RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ApplyComponentVersion()
        {
            _bind.UpdateComponentVersion(_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetComponentVersion()
            => _bind.GetComponentVersion(_id);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe int IndexOf(ref T item, int count)
        {
#if !ANOTHERECS_RELEASE
            if (!IsValide)
            {
                throw new DArrayInvalideException(this.GetType());
            }
#endif
            var array = ReadUnsafe();

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
#if !ANOTHERECS_RELEASE
            if (!IsValide)
            {
                throw new DArrayInvalideException(this.GetType());
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

            var data = ReadUnsafe();
            var iMax = Math.Min(array.Length - arrayIndex, count);
            for (int i = 0; i < iMax; ++i)
            {
                array[i + arrayIndex] = data[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateVersion()
        {
            _bind.UpdateVersion(_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe T* ReadUnsafe()
            => _bind.Read<T>(_id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe T* GetUnsafe()
            => _bind.Get<T>(_id);

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

#if !ANOTHERECS_RELEASE
        private void Validate()
        {
            if (!ComponentUtils.IsSimple(typeof(T)))
            {
                throw new DArraySimpleException(typeof(T));
            }
        }
#endif


        public struct Enumerator : IEnumerator<T>
        {
            private readonly DArray<T> _data;
            private int _current;
            private readonly int _length;
#if !ANOTHERECS_RELEASE
            private readonly int _version;
#endif
            public Enumerator(ref DArray<T> data)
            {
                _data = data;
                _length = _data.Length;
                _current = -1;
#if !ANOTHERECS_RELEASE
                _version = _data.GetComponentVersion();  //TODO SER error
#endif
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
#if !ANOTHERECS_RELEASE
                    if (_version != _data.GetComponentVersion())
                    {
                        throw new CollectionWasModifiedException();
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
