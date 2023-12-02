using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{    
    public struct NMultiDictionary<TKey, TValue, THashProvider> : IDisposable, ISerialize
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged, IEquatable<TValue>
        where THashProvider : struct, IHash<TKey, uint>
    {
        private NArray<int> _buckets;
        private NArray<Entry> _entries;
        private uint _count;
        private int _freeList;
        private uint _freeCount;
        private THashProvider _hashProvider;

        public NMultiDictionary(uint capacity)
        {
            uint size = HashHelpers.GetPrime(capacity);
            _buckets = new NArray<int>(size);
            for (uint i = 0; i < _buckets.Length; i++)
            {
                _buckets.GetRef(i) = -1;
            }
            _entries = new NArray<Entry>(size);
            _freeList = -1;
            _count = 0;
            _freeCount = 0;
            _hashProvider = default;
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
                for (int i = 0; i < _buckets.Length; i++)
                {
                    _buckets.GetRef(i) = -1;
                }
                _entries.Clear(_count);
                _freeList = -1;
                _count = 0;
                _freeCount = 0;
            }
        }

        public bool ContainsKey(TKey key)
            => FindEntry(key) >= 0;

        public bool ContainsValue(TValue value)
        {
            for (int i = 0; i < _count; i++)
            {
                ref var entry = ref _entries.GetRef(i);
                if (!entry.key.Equals(default) && entry.value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        private int FindEntry(TKey key)
        {
            for (int i = _buckets.GetRef(_hashProvider.GetHash(ref key) % _buckets.Length); i >= 0; i = _entries.GetRef(i).next)
            {
                if (_entries.GetRef(i).key.Equals(key))
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
           
            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = _entries.GetRef(index).next;
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

            ref var entry = ref _entries.GetRef(index);
            entry.next = _buckets.Get(targetBucket);
            entry.key = key;
            entry.value = value;
            _buckets.GetRef(targetBucket) = index;
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
            _buckets.Dispose();

            var newBuckets = new NArray<int>(newSize);
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets.GetRef(i) = -1;
            }

            for (int i = 0; i < _count; i++)
            {
                ref var entry = ref _entries.GetRef(i);
                if (!entry.value.Equals(default))
                {
                    ulong bucket = _hashProvider.GetHash(ref entry.key) % newSize;
                    entry.next = newBuckets.Get(bucket);
                    newBuckets.GetRef(bucket) = i;
                }
            }
            _buckets = newBuckets;
        }

        public bool Remove(uint key, uint value)
        {
            uint bucket = key % _buckets.Length;
            int last = -1;

            for (int i = _buckets.Get(bucket); i >= 0; last = i, i = _entries.GetRef(i).next)
            {
                ref var entry = ref _entries.GetRef(i);
                if (entry.key.Equals(key) && entry.value.Equals(value))
                {
                    if (last < 0)
                    {
                        _buckets.GetRef(bucket) = entry.next;
                    }
                    else
                    {
                        _entries.GetRef(last).next = entry.next;
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
            _buckets.Pack(ref writer);
            _entries.Pack(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_freeCount);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _buckets.Unpack(ref reader);
            _entries.Unpack(ref reader);

            _count = reader.ReadUInt32();
            _freeCount = reader.ReadUInt32();
            _freeList = reader.ReadInt32();
        }

        private struct Entry
        {
            public int next;            // Index of next entry, -1 if last
            public TKey key;            // Key of entry
            public TValue value;        // Value of entry
        }


        public struct ValuesByKeyEnumerable : IEnumerable<TValue>
        {
            private readonly NMultiDictionary<TKey, TValue, THashProvider> _data;
            private readonly uint _key;

            public ValuesByKeyEnumerable(ref NMultiDictionary<TKey, TValue, THashProvider> data, uint key)
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
                private readonly NMultiDictionary<TKey, TValue, THashProvider> _data;
                private readonly int _bucket;
                private readonly uint _key;
                private int _index;
                private TValue _current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ValuesByKeyEnumerator(in NMultiDictionary<TKey, TValue, THashProvider> data, uint key)
                {
                    _data = data;
                    _bucket = _data._buckets.GetRef(key % _data._buckets.Length);
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
                        ref var entry = ref _data._entries.GetRef(_index);
                        if (entry.key.Equals(_key))
                        {
                            _current = entry.value;
                            _index = _data._entries.GetRef(_index).next;
                            return true;
                        }
                        _index = _data._entries.GetRef(_index).next;
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