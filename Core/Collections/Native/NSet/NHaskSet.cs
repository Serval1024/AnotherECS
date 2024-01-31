using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct NHashSet<TAllocator, TValue, THashProvider> : INative, ISerialize, IEnumerable<TValue>, IRepairMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TValue : unmanaged, IEquatable<TValue>
        where THashProvider : struct, IHashProvider<TValue, uint>
    {
        private const uint _EMPTY = 0x8000_0000;
        private const uint _MASK = 0x7FFFFFFF;

        private NArray<TAllocator, int> _buckets;
        private NArray<TAllocator, Slot> _slots;

        private uint _count;
        private int _lastIndex;
        private int _freeList;
        private THashProvider _hashProvider;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsValid && _slots.IsValid;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsDirty || _slots.IsDirty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSet(TAllocator* allocator, INArray<TValue> list)
        {
            this = new NHashSet<TAllocator, TValue, THashProvider>(allocator, list.Length);
            foreach (var element in list)
            {
                Add(element);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSet(TAllocator* allocator, uint capacity)
        {
            _count = 0;
            _lastIndex = 0;
            _freeList = -1;

            var size = HashHelpers.GetPrime(capacity);
            _buckets = new NArray<TAllocator, int>(allocator, size);
            _slots = new NArray<TAllocator, Slot>(allocator, size);
            _hashProvider = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint elementCount)
        {
            if (IsAllocatorValid())
            {
                Dispose();
                this = new NHashSet<TAllocator, TValue, THashProvider>(GetAllocator(), elementCount);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TValue item)
        {
            var hashCode = _hashProvider.GetHash(ref item) & _MASK;
            
            for (int i = _buckets.Read(hashCode % _buckets.Length) - 1; i >= 0; i = _slots.ReadRef(i).next)
            {
                if (hashCode == _slots.ReadRef(i).hashCode && _slots.ReadRef(i).item.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TValue item)
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new ArgumentException();
            }
#endif
            _slots.Dirty();
            _buckets.Dirty();

            var hashCode = _hashProvider.GetHash(ref item) & _MASK;
            uint bucketId = hashCode % _buckets.Length;

            int index;
            if (_freeList >= 0)
            {
                index = _freeList;
                _freeList = _slots.ReadRef(index).next;
            }
            else
            {
                if (_lastIndex == _slots.Length)
                {
                    Resize();
                    bucketId = hashCode % _buckets.Length;
                }
                index = _lastIndex++;
            }
            ref var slot = ref _slots.ReadRef(index);
            ref var bucket = ref _buckets.ReadRef(bucketId);
            slot.hashCode = hashCode;
            slot.item = item;
            slot.next = bucket - 1;
            bucket = index + 1;
            ++_count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TValue item)
        {
            _slots.Dirty();
            _buckets.Dirty();

            uint hashCode = _hashProvider.GetHash(ref item);
            uint bucketId = hashCode % _buckets.Length;
            int lastId = -1;
            for (int i = _buckets.Get(bucketId) - 1; i >= 0; lastId = i, i = _slots.ReadRef(i).next)
            {
                if (_slots.ReadRef(i).hashCode == hashCode && _slots.ReadRef(i).item.Equals(item))
                {
                    if (lastId < 0)
                    {
                        _buckets.ReadRef(bucketId) = _slots.ReadRef(i).next + 1;
                    }
                    else
                    {
                        _slots.ReadRef(lastId).next = _slots.ReadRef(i).next;
                    }
                    ref var slot = ref _slots.ReadRef(i);
                    slot.hashCode = _EMPTY;
                    slot.item = default;
                    slot.next = _freeList;

                    if (--_count == 0)
                    {
                        _lastIndex = 0;
                        _freeList = -1;
                    }
                    else
                    {
                        _freeList = i;
                    }
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_lastIndex > 0)
            {
                _buckets.Clear();
                _slots.Clear(0, (uint)_lastIndex);

                _lastIndex = 0;
                _count = 0;
                _freeList = -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _buckets.PackBlittable(ref writer);
            _slots.Pack(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_lastIndex);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _buckets.UnpackBlittable(ref reader);
            _slots.Unpack(ref reader);

            _count = reader.ReadUInt32();
            _lastIndex = reader.ReadInt32();
            _freeList = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer)
        {
            _buckets.PackBlittable(ref writer);
            _slots.PackBlittable(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_lastIndex);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            _buckets.UnpackBlittable(ref reader);
            _slots.UnpackBlittable(ref reader);

            _count = reader.ReadUInt32();
            _lastIndex = reader.ReadInt32();
            _freeList = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize()
        {
            uint newSize = HashHelpers.ExpandPrime(Count);
#if !ANOTHERECS_RELEASE
            if (newSize <= Count)
            {
                throw new ArgumentException();
            }
#endif
            SetCapacity(newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(uint newSize)
        {
            _slots.Resize(newSize);

            var newBuckets = new NArray<TAllocator, int>(_buckets.GetAllocator(), newSize);
            _buckets.Dispose();

            var lastIndex = _lastIndex;

            for (int i = 0; i < lastIndex; i++)
            {
                var slot = _slots.ReadRef(i);
                uint bucket = slot.hashCode % newSize;
                slot.next = newBuckets.ReadRef(bucket) - 1;
                newBuckets.ReadRef(bucket) = i + 1;
            }

            _buckets = newBuckets;
        }

        public TValue Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif
            int counterIndex = 0;
            while (counterIndex < _lastIndex)
            {
                if (_slots.ReadRef(counterIndex).hashCode < _EMPTY)
                {
                    if (counterIndex == index)
                    {
                        return _slots.ReadRef(counterIndex).item;
                    }
                    ++counterIndex;
                }
            }
            
            throw new IndexOutOfRangeException(nameof(index));
        }

        public void Set(uint index, TValue value)
        {
#if !ANOTHERECS_RELEASE
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif
            int counterIndex = 0;
            while (counterIndex < _lastIndex)
            {
                if (_slots.ReadRef(counterIndex).hashCode < _EMPTY)
                {
                    if (counterIndex == index)
                    {
                        var oldValue = _slots.ReadRef(counterIndex).item;
                        Remove(oldValue);
                        Add(value);
                        return;
                    }
                    ++counterIndex;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _buckets.Dispose();
            _slots.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges()
        {
            _buckets.EnterCheckChanges();
            _slots.EnterCheckChanges();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges()
        {
            var result = false;
            result |= _buckets.ExitCheckChanges();
            result |= _slots.ExitCheckChanges();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _buckets, ref repairMemoryContext);
            RepairMemoryCaller.Repair(ref _slots, ref repairMemoryContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsAllocatorValid()
            => _buckets.IsAllocatorValid() && _slots.IsAllocatorValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TAllocator* GetAllocator()
            => _buckets.GetAllocator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetAllocator(TAllocator* allocator)
        {
            _buckets.SetAllocator(allocator);
            _slots.SetAllocator(allocator);
        }


        private struct Slot
        {
            public uint hashCode;
            public int next;      // Index of next entry, -1 if last
            public TValue item;
        }

        public struct Enumerator : IEnumerator<TValue>, IEnumerator
        {
            private NHashSet<TAllocator, TValue, THashProvider> _data;
            private int _index;
            private TValue _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref NHashSet<TAllocator, TValue, THashProvider> data)
            {
                _data = data;
                _index = 0;
                _current = default;
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.IsValid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_index < _data._lastIndex)
                {
                    if (_data._slots.ReadRef(_index).hashCode < _EMPTY)
                    {
                        _current = _data._slots.ReadRef(_index).item;
                        ++_index;
                        return true;
                    }
                    ++_index;
                }
                return false;
            }

            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _data._lastIndex + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }
}