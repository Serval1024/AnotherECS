using AnotherECS.Core.Allocators;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NMultiDictionaryZero<TAllocator, TKey, TValue, THashProvider> : INative, ISerialize, IRepairMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged, IEquatable<TValue>
        where THashProvider : struct, IHashProvider<TKey, uint>
    {
        private NArray<TAllocator, int> _buckets;
        private NArray<TAllocator, Entry> _entries;

        private uint _count;
        private uint _freeCount;
        private int _freeList;

        private THashProvider _hashProvider;

        public NMultiDictionaryZero(TAllocator* allocator, uint capacity)
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

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsValid && _entries.IsValid;
        }

        public uint Count
             => _count - _freeCount;

        public void Add(TKey key, TValue value)
        {
            Insert(key, value);
        }

        public void Clear()
        {
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

        public bool ContainsKey(TKey key)
            => FindEntry(key) >= 0;

        public bool ContainsValue(TValue value)
        {
            var count = _count;
            for (int i = 0; i < count; i++)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (!entry.key.Equals(default) && entry.value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        private int FindEntry(TKey key)
        {
            for (int i = _buckets.ReadRef(_hashProvider.GetHash(ref key) % _buckets.Length); i >= 0; i = _entries.ReadRef(i).next)
            {
                if (_entries.ReadRef(i).key.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(TKey key, TValue value)
        {
            ulong targetBucket = _hashProvider.GetHash(ref key) % _buckets.Length;

            _entries.Dirty();
            _buckets.Dirty();

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
                    targetBucket = _hashProvider.GetHash(ref key) % _buckets.Length;
                }
                index = (int)_count;
                ++_count;
            }

            ref var entry = ref _entries.ReadRef(index);
            entry.next = _buckets.Get(targetBucket);
            entry.key = key;
            entry.value = value;
            _buckets.ReadRef(targetBucket) = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize()
        {
            Resize(HashHelpers.GetPrime(_count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(uint newSize)
        {
            _entries.Resize(newSize);

            var newBuckets = new NArray<TAllocator, int>(_buckets.GetAllocator(), newSize);
            for (uint i = 0; i < newBuckets.Length; ++i)
            {
                newBuckets.ReadRef(i) = -1;
            }

            _buckets.Dispose();

            var count = _count;

            for (uint i = 0; i < count; ++i)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (!entry.value.Equals(default))
                {
                    ulong bucket = _hashProvider.GetHash(ref entry.key) % newSize;
                    entry.next = newBuckets.Read(bucket);
                    newBuckets.ReadRef(bucket) = (int)i;
                }
            }
            _buckets = newBuckets;
        }

        public bool Remove(uint key, uint value)
        {
            uint bucket = key % _buckets.Length;
            int last = -1;

            _entries.Dirty();
            _buckets.Dirty();


            for (int i = _buckets.Get(bucket); i >= 0; last = i, i = _entries.ReadRef(i).next)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (entry.key.Equals(key) && entry.value.Equals(value))
                {
                    if (last < 0)
                    {
                        _buckets.ReadRef(bucket) = entry.next;
                    }
                    else
                    {
                        _entries.ReadRef(last).next = entry.next;
                    }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValuesByKeyEnumerable GetValues(uint key)
            => new(ref this, key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _buckets, ref repairMemoryContext);
            RepairMemoryCaller.Repair(ref _entries, ref repairMemoryContext);
        }

        private struct Entry
        {
            public int next;            // Index of next entry, -1 if last
            public TKey key;            // Key of entry
            public TValue value;        // Value of entry
        }


        public struct ValuesByKeyEnumerable : IEnumerable<TValue>
        {
            private readonly NMultiDictionaryZero<TAllocator, TKey, TValue, THashProvider> _data;
            private readonly uint _key;

            public ValuesByKeyEnumerable(ref NMultiDictionaryZero<TAllocator, TKey, TValue, THashProvider> data, uint key)
            {
                _data = data;
                _key = key;
            }

            public IEnumerator<TValue> GetEnumerator()
                => new ValuesByKeyEnumerator(_data, _key);

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            public struct ValuesByKeyEnumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly NMultiDictionaryZero<TAllocator, TKey, TValue, THashProvider> _data;
                private readonly int _bucket;
                private readonly uint _key;
                private int _index;
                private TValue _current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ValuesByKeyEnumerator(in NMultiDictionaryZero<TAllocator, TKey, TValue, THashProvider> data, uint key)
                {
                    _data = data;
                    _bucket = _data._buckets.ReadRef(key % _data._buckets.Length);
                    _index = _bucket;
                    _current = default;
                    _key = key;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() { }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    while (_index >= 0)
                    {
                        ref var entry = ref _data._entries.ReadRef(_index);
                        if (entry.key.Equals(_key))
                        {
                            _current = entry.value;
                            _index = _data._entries.ReadRef(_index).next;
                            return true;
                        }
                        _index = _data._entries.ReadRef(_index).next;
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
                        if (_index == 0 || _index == _data._count)
                        {
                            throw new InvalidOperationException();
                        }
                        return Current;
                    }
                }

                void IEnumerator.Reset()
                {
                    _index = _bucket;
                    _current = default;
                }
            }
        }
    }
}