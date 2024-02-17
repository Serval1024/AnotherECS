using AnotherECS.Core.Allocators;
using AnotherECS.Core.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NDictionary<TAllocator, TKey, TValue, THashProvider> : INative, ISerialize, IEnumerable<Pair<TKey, TValue>>, IRepairMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where THashProvider : struct, IHashProvider<TKey, uint>
    {
        private const uint _EMPTY = 0x8000_0000;
        private const uint _MASK = 0x7FFFFFFF;

        private NArray<TAllocator, int> _buckets;
        private NArray<TAllocator, Entry> _entries;

        private uint _count;
        private uint _freeCount;
        private int _freeList;

        private THashProvider _hashProvider;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsValid && _entries.IsValid;
        }

        public uint Count
            => _count - _freeCount;

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsDirty || _entries.IsDirty;
        }

        public NDictionary(TAllocator* allocator, uint capacity)
        {
            _count = 0;
            _freeCount = 0;
            _freeList = -1;

            uint size = HashHelpers.GetPrime(capacity);
            _buckets = new NArray<TAllocator, int>(allocator, size);
            for (uint i = 0; i < _buckets.Length; i++)
            {
                _buckets.ReadRef(i) = -1;
            }
            _entries = new NArray<TAllocator, Entry>(allocator, size);
            _hashProvider = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint elementCount)
        {
            if (IsAllocatorValid())
            {
                Dispose();
                this = new NDictionary<TAllocator, TKey, TValue, THashProvider>(GetAllocator(), elementCount);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfBroken(this);
#endif
                int i = FindEntry(key);
                if (i >= 0)
                {
                    return _entries.ReadRef(i).value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            Insert(key, value, true);
        }

        public void Clear()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            if (_count > 0)
            {
                _buckets.Dirty();

                for (int i = 0; i < _buckets.Length; i++)
                {
                    _buckets.ReadRef(i) = -1;
                }
                _entries.Clear(0, _count);

                _count = 0;
                _freeCount = 0;
                _freeList = -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(NDictionary<TAllocator, TKey, TValue, THashProvider> source)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(source);
#endif
            if (_buckets.Length != source._buckets.Length)
            {
                _buckets.Allocate(source._buckets.Length);
            }
            if (_entries.Length != source._entries.Length)
            {
                _entries.Allocate(source._entries.Length);
            }
            _buckets.CopyFrom(source._buckets);
            _entries.CopyFrom(source._entries);

            _count = source._count;
            _freeCount = source._freeCount;
            _freeList = source._freeList;
        }

        public bool ContainsKey(TKey key)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            var count = _count;
            for (int i = 0; i < count; i++)
            {
                if (_entries.ReadRef(i).hashCode < _EMPTY && _entries.ReadRef(i).value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindEntry(TKey key)
        {
            var hashCode = _hashProvider.GetHash(ref key) & _MASK;
            for (int i = _buckets.Get(hashCode % _buckets.Length); i >= 0; i = _entries.ReadRef(i).next)
            {
                if (hashCode == _entries.ReadRef(i).hashCode && _entries.ReadRef(i).key.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(TKey key, TValue value, bool add)
        {
            var hashCode = _hashProvider.GetHash(ref key) & _MASK;
            uint targetBucket = hashCode % _buckets.Length;

            _entries.Dirty();
            _buckets.Dirty();


            for (int i = _buckets.Read(targetBucket); i >= 0; i = _entries.ReadRef(i).next)
            {
                if (hashCode == _entries.ReadRef(i).hashCode && _entries.ReadRef(i).key.Equals(key))
                {
#if !ANOTHERECS_RELEASE
                    if (add)
                    {
                        throw new ArgumentException();
                    }
#endif
                    _entries.ReadRef(i).value = value;
                    return;
                }
            }
            
            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = _entries.ReadRef(index).next;
                --_freeCount;
            }
            else
            {
                if (_count == _entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % _buckets.Length;
                }
                index = (int)_count;
                ++_count;
            }

            ref var entry = ref _entries.ReadRef(index);
            entry.hashCode = hashCode;
            entry.next = _buckets.Get(targetBucket);
            entry.key = key;
            entry.value = value;
            _buckets.ReadRef(targetBucket) = index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _buckets.EnterCheckChanges();
            _entries.EnterCheckChanges();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            var result = false;
            result |= _buckets.ExitCheckChanges();
            result |= _entries.ExitCheckChanges();
            return result;
        }

      
        public bool Remove(TKey key)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            uint hashCode = _hashProvider.GetHash(ref key) & _MASK;
            uint bucket = hashCode % _buckets.Length;
            int last = -1;

            _entries.Dirty();
            _buckets.Dirty();

            for (int i = _buckets.Read(bucket); i >= 0; last = i, i = _entries.ReadRef(i).next)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (hashCode == _entries.ReadRef(i).hashCode && entry.key.Equals(key))
                {
                    if (last < 0)
                    {
                        _buckets.ReadRef(bucket) = entry.next;
                    }
                    else
                    {
                        _entries.ReadRef(last).next = entry.next;
                    }
                    entry.hashCode = _EMPTY;
                    entry.next = _freeList;
                    entry.key = default;
                    entry.value = default;
                    _freeList = i;
                    ++_freeCount;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = _entries.ReadRef(i).value;
                return true;
            }
            value = default;
            return false;
        }

        public TValue* TryGetPtrValue(TKey key)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            int i = FindEntry(key);
            if (i >= 0)
            {
                return &_entries.ReadPtr(i)->value;
                
            }
            return null;
        }

        public TValue Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif
            int cIndex = 0;
            var count = Count;
            while (cIndex < count)
            {
                if (_entries.ReadRef(cIndex).hashCode < _EMPTY)
                {
                    if (index == cIndex)
                    {
                        return _entries.ReadRef(cIndex).value;
                    }
                    ++cIndex;       
                }
            }

            throw new IndexOutOfRangeException(nameof(index));
        }

        public void Set(uint index, TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif

            int cIndex = 0;
            var count = Count;
            while (cIndex < count)
            {
                if (_entries.ReadRef(cIndex).hashCode < _EMPTY)
                {
                    if (index == cIndex)
                    {
                        var key = _entries.ReadRef(cIndex).key;
                        Remove(key);
                        Add(key, value);
                        return;
                    }
                    ++cIndex;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachValue<TIterator>(TIterator iterator)
            where TIterator : struct, IIterator<TValue>
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            int index = 0;
            while (index < Count)
            {
                if (_entries.ReadRef(index).hashCode < _EMPTY)
                {
                    iterator.Each(ref _entries.ReadRef(index).value);
                }
                ++index;
            }
        }

        public void Dispose()
        {
            _buckets.Dispose();
            _entries.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _buckets.PackBlittable(ref writer);
            _entries.Pack(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_freeCount);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _buckets.UnpackBlittable(ref reader);
            _entries.Unpack(ref reader);

            _count = reader.ReadUInt32();
            _freeCount = reader.ReadUInt32();
            _freeList = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer)
        {
            _buckets.PackBlittable(ref writer);
            _entries.PackBlittable(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_freeCount);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            _buckets.UnpackBlittable(ref reader);
            _entries.UnpackBlittable(ref reader);

            _count = reader.ReadUInt32();
            _freeCount = reader.ReadUInt32();
            _freeList = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<Pair<TKey, TValue>> IEnumerable<Pair<TKey, TValue>>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _buckets, ref repairMemoryContext);
            RepairMemoryCaller.Repair(ref _entries, ref repairMemoryContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsAllocatorValid()
           => _buckets.IsAllocatorValid() && _entries.IsAllocatorValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TAllocator* GetAllocator()
            => _buckets.GetAllocator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetAllocator(TAllocator* allocator)
        {
            _buckets.SetAllocator(allocator);
            _entries.SetAllocator(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize()
        {
            Resize(HashHelpers.ExpandPrime(_count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(uint size)
        {
            _entries.Resize(size);

            var newBuckets = new NArray<TAllocator, int>(_buckets.GetAllocator(), size);
            for (uint i = 0; i < newBuckets.Length; ++i)
            {
                newBuckets.ReadRef(i) = -1;
            }

            _buckets.Dispose();

            var count = _count;

            for (int i = 0; i < count; ++i)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (!entry.value.Equals(default))
                {
                    uint bucket = entry.hashCode % size;
                    entry.next = newBuckets.Read(bucket);
                    newBuckets.ReadRef(bucket) = i;
                }
            }

            _buckets = newBuckets;
        }


        private struct Entry
        {
            public uint hashCode;
            public int next;            // Index of next entry, -1 if last
            public TKey key;            // Key of entry
            public TValue value;        // Value of entry
        }

        public struct Enumerator : IEnumerator<Pair<TKey, TValue>>, IEnumerator
        {
            private NDictionary<TAllocator, TKey, TValue, THashProvider> _data;
            private uint _index;
            private Pair<TKey, TValue> _current;

            internal Enumerator(ref NDictionary<TAllocator, TKey, TValue, THashProvider> data)
            {
                _data = data;
                _index = 0;
                _current = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_index < _data.Count)
                {
                    if (_data._entries.ReadRef(_index).hashCode < _EMPTY)
                    {
                        ref var entry = ref _data._entries.ReadRef(_index);
                        _current.key = entry.key;
                        _current.value = entry.value;
                        ++_index;
                        return true;
                    }
                    ++_index;
                }
                _index = _data.Count + 1;
                _current = default;
                return false;
            }

            public Pair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _data.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }

    public struct Pair<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}