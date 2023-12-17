using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Core.Collection;
using AnotherECS.Exceptions;
using AnotherECS.Serializer;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DList<T> : IInject<NPtr<HAllocator>>, ICList<T>, IList<T>, IEnumerable<T>, ISerialize, IRebindMemoryHandle
        where T : unmanaged
    {
        private DArray<T> _data;
        private uint _count;

#if !ANOTHERECS_RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<NPtr<HAllocator>>.Construct(NPtr<HAllocator> allocator)
        { 
            InjectUtils.Contruct(ref _data, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        {
            InjectUtils.Decontruct(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(NPtr<HAllocator> allocator)
        {
            _data.Construct(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct()
        {
            _data.Deconstruct();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }
#endif

        public bool IsValide
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValide;
        }

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
            internal set => _count = value;
        }

        public bool IsReadOnly
            => false;

        int ICollection<T>.Count => (int)Count;

        public T this[int index]
        {
            get => this[(uint)index];
            set => this[(uint)index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read(uint index)
        {
            if (index >= Count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {Count}.");
            }
            return ref _data.Read(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(uint index)
        {
            if (index >= Count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {Count}.");
            }
            return ref _data.Get(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
            => Set(index, ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref T item)
        {
#if !ANOTHERECS_RELEASE
            FArrayHelper.ThrowIfOutOfRange(index, _count);
#endif
            _data.Set(index, ref item);
        }

        public void Add(T item)
        { 
            Add(ref item);
        }

        public void Add(ref T item)
        {
            if (Count == Capacity)
            {
                Resize((Count + 1) << 1);
            }

            _data.Set(Count++, ref item);
        }

        public void Insert(uint index, T item)
        {
            Insert(index, ref item);
        }

        public void Insert(uint index, ref T item)
        {
#if !ANOTHERECS_RELEASE
            FArrayHelper.ThrowIfOutOfRange(index, Count);
            if (!_data.IsValide)
            {
                throw new DArrayInvalideException(_data.GetType());
            }
#endif
            if (index == Count - 1)
            {
                Add(ref item);
            }
            else
            {
                if (Count == Capacity)
                {
                    Resize((Count + 1) << 1);
                }

                MoveRigth(index, Count);
                _data.Set(index, ref item);
                ++Count;
            }
        }

        public void RemoveAt(uint index)
        {
#if !ANOTHERECS_RELEASE
            FArrayHelper.ThrowIfOutOfRange(index, Count);
            if (!_data.IsValide)
            {
                throw new DArrayInvalideException(_data.GetType());
            }
#endif
            if (index == Count - 1)
            {
                RemoveLast();
            }
            else
            {
                MoveLeft(index, Count);
                _data.Set(index + Count - 1, default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void MoveRigth(uint index, uint count)
        {
            if (count == 0)
            {
                return;
            }

            var dataPtr = _data.ReadPtr();
            for (uint i = count - 1, iMax = index + 1; i >= iMax; --i)
            {
                dataPtr[i] = dataPtr[i - 1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void MoveLeft(uint index, uint count)
        {
            if (count == 0)
            {
                return;
            }

            var dataPtr = _data.ReadPtr();
            for (uint i = index, iMax = count - 1; i < iMax; ++i)
            {
                dataPtr[i] = dataPtr[i + 1];
            }
        }

        public unsafe int IndexOf(T item)
            => IndexOf(ref item);

        public unsafe int IndexOf(ref T item)
            => _data.IndexOf(ref item, Count);

        public bool Contains(T item)
            => Contains(ref item);

        public bool Contains(ref T item)
            => _data.IndexOf(ref item, Count) != -1;

        public bool Remove(T item)
            => Remove(ref item);

        public bool Remove(ref T item)
        {
            var index = _data.IndexOf(ref item, Count);
            if (index != -1)
            {
                RemoveAt((uint)index);
                return true;
            }
            return false;
        }

        public void CopyTo(T[] array, uint arrayIndex)
        {
            _data.CopyTo(array, arrayIndex, Count);
        }

        public void RemoveLast()
        {
            if (Count != 0)
            {
                this[--Count] = default;
            }
        }

        public void Clear()
        {
            _data.Clear();
        }

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, ref value);
        }

        public void Resize(uint capacity)
        {
            _data.Resize(capacity);
        }

        public void ExtendToCapacity()
        {
            while (Count != Capacity)
            {
                _data.Set(Count++, default);
            }
        }

        public IEnumerator<T> GetEnumerator()
          => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
            writer.Write(Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
            _count = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
            => _data.AsSpan()[..(int)Count];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe T* ReadPtr()
            => _data.ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe T* GetPtr()
            => _data.GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();

        object ICList.Get(uint index)
            => this[index];

        void ICList.Set(uint index, object value)
        {
            this[index] = (T)value;
        }

        void ICList.Add(object value)
        {
            Add((T)value);
        }

        public void Insert(int index, T item)
        {
            Insert((uint)index, item);
        }

        public void RemoveAt(int index)
        {
            RemoveAt((uint)index);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, (uint)arrayIndex);
        }


        public struct Enumerator : IEnumerator<T>
        {
            private readonly DList<T> _data;
            private uint _current;
            private readonly uint _count;

            public Enumerator(ref DList<T> data)
            {
                _data = data;
                _count = _data.Count;
                _current = uint.MaxValue;

                _data.EnterCheckChanges();
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data[_current];
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_current < _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            { 
                _current = uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
            }
        }
    }
}