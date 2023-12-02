using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using AnotherECS.Core;
using AnotherECS.Serializer;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DString : IInject<DArrayCaller>, ICString<char>, IEnumerable<char>, ISerialize
    {
        private DList<char> _data;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(DArrayCaller bind)
        {
            _data.Construct(bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct()
        {
            _data.Deconstruct();
        }
#endif

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Capacity;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }
       
        public static implicit operator string(DString fstring) => fstring.ToString();

        public static bool operator ==(DString a, DString b)
            => a.Equals(ref b);

        public static bool operator !=(DString a, DString b)
            => !a.Equals(ref b);

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _data[index] = value;
        }

        public unsafe void Set(string str)
        {
            if (str.Length > Capacity)
            {
                _data.Resize(str.Length);
            }
            var dataPtr = _data.ReadUnsafe();
            for (int i = 0; i < str.Length; ++i)
            {
                dataPtr[i] = str[i];
            }
            _data.Count = str.Length;
        }

        public unsafe void Concat(string str)
        {
            var max = Length + str.Length;
            if (max > Capacity)
            {
                _data.Resize(max);
            }
            var dataPtr = _data.ReadUnsafe();
            for (int i = Length; i < max; ++i)
            {
                dataPtr[i] = str[i];
            }
            _data.Count = max;
        }

        public unsafe void Concat(DString str)
        {
            var max = Length + str.Length;
            if (max > Capacity)
            {
                _data.Resize(max);
            }
            var dataPtr = _data.ReadUnsafe();
            var strPtr = str._data.ReadUnsafe();
            for (int i = Length; i < max; ++i)
            {
                dataPtr[i] = str[i];
            }
            _data.Count = max;
        }

        public override unsafe string ToString()
        {
#pragma warning disable CS0162
            if (Capacity <= 16)
            {
                var text = string.Empty;
                var dataPtr = _data.ReadUnsafe();
                for (int i = 0; i < Length; ++i)
                {
                    text += dataPtr[i];
                }
                return text;
            }
            else
            {
                var stringBuilder = new StringBuilder();
                var dataPtr = _data.ReadUnsafe();
                for (int i = 0; i < Length; ++i)
                {
                    stringBuilder.Append(dataPtr[i]);
                }
                return stringBuilder.ToString();
#pragma warning restore CS0162
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DString fString)
            {
                return Equals(fString);
            }
            return false;
        }

        public bool Equals(DString other)
            => Equals(ref other);

        public unsafe bool Equals(ref DString other)
        {
            if (Length == other.Length)
            {
                var dataPtr = _data.ReadUnsafe();
                var otherDataPtr = other._data.ReadUnsafe();
                for (int i = 0; i < Length; ++i)
                {
                    if (dataPtr[i] != otherDataPtr[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override unsafe int GetHashCode()
        {
            HashCode hash = default;
            hash.Add(Length);
            var dataPtr = _data.ReadUnsafe();
            for (int i = 0; i < Length; ++i)
            {
                hash.Add(dataPtr[i]);
            }

            return hash.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<char> GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<char> AsSpan()
           => _data.AsSpan();

        public struct Enumerator : IEnumerator<char>
        {
            private readonly DString _data;
            private int _current;

            public Enumerator(ref DString data)
            {
                _data = data;
                _current = -1;
            }

            public char Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data[_current];
            }

            object IEnumerator.Current
                => _data[_current];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_current < _data.Length;

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