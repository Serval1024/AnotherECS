using AnotherECS.Collections.Exceptions;
using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DString : IInject<WPtr<AllocatorSelector>>, IEquatable<DString>, ICString<char>, ICollection<char>, IEnumerable<char>, ISerialize, IValid, IRepairMemoryHandle
    {
        private DList<char> _data;


        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Capacity;
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        public uint Count => Length;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }


        public static implicit operator string(DString fstring) => fstring.ToString();

        public static bool operator ==(DString a, DString b)
            => a.Equals(ref b);

        public static bool operator !=(DString a, DString b)
            => !a.Equals(ref b);

        public char this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _data[index] = value;
        }

        public object Get(uint index)
            => this[index];

        public void Set(uint index, object value)
        {
            this[index] = (char)value;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public unsafe void Set(string str)
        {
            if (str.Length > Capacity)
            {
                _data.Resize((uint)str.Length);
            }
            var dataPtr = _data.GetPtr();
            for (int i = 0; i < str.Length; ++i)
            {
                dataPtr[i] = str[i];
            }
            _data.Count = (uint)str.Length;
        }

        public unsafe void Concat(string str)
        {
            var offset = (int)Length;
            var max = offset + str.Length;
            if (max > Capacity)
            {
                _data.Resize((uint)max);
            }
            var dataPtr = _data.GetPtr();
            for (int i = (int)Length; i < max; ++i)
            {
                dataPtr[i + offset] = str[i];
            }
            _data.Count = (uint)max;
        }

        public unsafe void Concat(DString str)
        {
            var offset = Length;
            var max = Length + str.Length;
            if (max > Capacity)
            {
                _data.Resize(max);
            }

            var dataPtr = _data.GetPtr();
            var strPtr = str._data.ReadPtr();

            for (uint i = Length; i < max; ++i)
            {
                dataPtr[i + offset] = strPtr[i];
            }
            _data.Count = max;
        }

        public override unsafe string ToString()
        {
            if (!_data.IsValid)
            {
                return string.Empty;
            }

#pragma warning disable CS0162
            if (Capacity <= 16)
            {
                var text = string.Empty;
                var dataPtr = _data.ReadPtr();
                for (uint i = 0; i < Length; ++i)
                {
                    text += dataPtr[i];
                }
                return text;
            }
            else
            {
                var stringBuilder = new StringBuilder();
                var dataPtr = _data.ReadPtr();
                for (uint i = 0; i < Length; ++i)
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
                var dataPtr = _data.ReadPtr();
                var otherDataPtr = other._data.ReadPtr();
                for (uint i = 0; i < Length; ++i)
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
            if (!IsValid)
            {
                return 0;
            }

            HashCode hash = default;
            hash.Add(Length);
            var dataPtr = _data.ReadPtr();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();

        #region declarations
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<WPtr<AllocatorSelector>>.Construct(
            [InjectMap(nameof(BAllocator), "allocatorType=1")]
            [InjectMap(nameof(HAllocator), "allocatorType=2")]
            WPtr<AllocatorSelector> allocator)
        {
            InjectUtils.Construct(ref _data, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        {
            InjectUtils.Deconstruct(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _data, ref repairMemoryContext);
        }
        #endregion

        #region declarations
        public struct Enumerator : IEnumerator<char>
        {
            private readonly DString _data;
            private uint _current;

            public Enumerator(ref DString data)
            {
                _data = data;
                _current = uint.MaxValue;

                if (_data.Length != 0)
                {
                    _data.EnterCheckChanges();
                }
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
                _current = uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() 
            {
                if (_data.Length != 0)
                {
                    ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
                }
            }
        }
        #endregion
    }
}