using AnotherECS.Collections.Exceptions;
using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections
{
    [ForceBlittable]
    public struct DList<TValue> : IInject<WPtr<AllocatorSelector>>, IListCollection<TValue>, IList<TValue>, IEnumerable<TValue>, ISerialize, IValid, IRepairMemoryHandle, IRepairStateId
        where TValue : unmanaged
    {
        private DArray<TValue> _data;
        private uint _count;

        public bool IsValid
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
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

        int System.Collections.Generic.ICollection<TValue>.Count => (int)Count;

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        public TValue this[int index]
        {
            get => this[(uint)index];
            set => this[(uint)index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint length)
        {
            _data.Allocate(length);
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate()
        {
            Allocate(DCollectionConst.DEFAULT_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _data.Deallocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (index >= Count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {Count}.");
            }
#endif
            return ref _data.Read(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (index >= Count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {Count}.");
            }
#endif
            return ref _data.Get(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, TValue value)
            => Set(index, ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            FArrayHelper.ThrowIfOutOfRange(index, _count);
#endif
            _data.Set(index, ref item);
        }

        public void Add(TValue item)
        { 
            Add(ref item);
        }

        public void Add(ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            if (Count == Capacity)
            {
                Resize((Count + 1) << 1);
            }

            _data.Set(Count++, ref item);
        }

        public void Insert(uint index, TValue item)
        {
            Insert(index, ref item);
        }

        public void Insert(uint index, ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            FArrayHelper.ThrowIfOutOfRange(index, Count);
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

                MoveRight(index, Count);
                _data.Set(index, ref item);
                ++Count;
            }
        }

        public void RemoveAt(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            FArrayHelper.ThrowIfOutOfRange(index, Count);
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
        private unsafe void MoveRight(uint index, uint count)
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

        public unsafe int IndexOf(TValue item)
            => IndexOf(ref item);

        public unsafe int IndexOf(ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.IndexOf(ref item, Count);
        }

        public bool Contains(TValue item)
            => Contains(ref item);

        public bool Contains(ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.IndexOf(ref item, Count) != -1;
        }

        public bool Remove(TValue item)
            => Remove(ref item);

        public bool Remove(ref TValue item)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            var index = _data.IndexOf(ref item, Count);
            if (index != -1)
            {
                RemoveAt((uint)index);
                return true;
            }
            return false;
        }

        public void CopyFrom(DList<TValue> source)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(source);
#endif
            _data.CopyFrom(source._data);
            _count = source._count;
        }

        public void CopyTo(TValue[] array, uint arrayIndex)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _data.CopyTo(array, arrayIndex, Count);
        }

        public void RemoveLast()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            if (Count != 0)
            {
                this[--Count] = default;
            }
        }

        public void Clear()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _data.Clear();
        }

        public TValue this[uint index]
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
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            while (Count != Capacity)
            {
                _data.Set(Count++, default);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
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
        public unsafe Span<TValue> AsSpan()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.AsSpan()[..(int)Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TValue* ReadPtr()
            => _data.ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TValue* GetPtr()
            => _data.GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();

        object ICollection.Get(uint index)
            => this[index];

        void ICollection.Set(uint index, object value)
        {
            this[index] = (TValue)value;
        }

        void IListCollection.Add(object value)
        {
            Add((TValue)value);
        }

        public void Insert(int index, TValue item)
        {
            Insert((uint)index, item);
        }

        public void RemoveAt(int index)
        {
            RemoveAt((uint)index);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            CopyTo(array, (uint)arrayIndex);
        }

        #region inner interfaces
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<WPtr<AllocatorSelector>>.Construct(
            [InjectMap(nameof(BAllocator), "allocatorType=1")]
            [InjectMap(nameof(HAllocator), "allocatorType=2")]
            WPtr<AllocatorSelector> allocator
            )
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

        bool IRepairStateId.IsRepairStateId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => typeof(IRepairStateId).IsAssignableFrom(typeof(TValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairStateId.RepairStateId(ushort stateId)
        {
            if (IsValid)
            {
                _data.RepairIdElement(stateId, 0, Count);
            }
        }
        #endregion

        #region declarations
        public struct Enumerator : IEnumerator<TValue>
        {
            private readonly DList<TValue> _data;
            private uint _current;
            private readonly uint _count;

            public Enumerator(ref DList<TValue> data)
            {
                _data = data;
                _count = _data.Count;
                _current = uint.MaxValue;

                if (_data.Count != 0)
                {
                    _data.EnterCheckChanges();
                }
            }

            public TValue Current
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
                if (_data.Count != 0)
                {
                    ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
                }
            }
        }
        #endregion
    }
}