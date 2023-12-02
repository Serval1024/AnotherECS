using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public struct NDictionary<TKey, TValue, THashProvider> : IDisposable, ISerialize
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged, IEquatable<TValue>
        where THashProvider : struct, IHash<TKey, ulong>
    {
        private NArray<int> _buckets;
        private NArray<Entry> _entries;
        private uint _count;
        private int _freeList;
        private uint _freeCount;
        private THashProvider _hashProvider;

        public NDictionary(uint capacity)
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

        public TValue this[TKey key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0)
                {
                    return _entries.GetRef(i).value;
                }
#if ANOTHERECS_RELEASE
                return default;
#else
                throw new System.Collections.Generic.KeyNotFoundException();
#endif
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
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
                if (!value.Equals(default) && _entries.GetRef(i).value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindEntry(TKey key)
        {
            for (int i = _buckets.GetRef(_hashProvider.GetHash(ref key) % _buckets.Length); i >= 0; i = _entries.GetRef(i).next)
            {
                if (_hashProvider.GetHash(ref key) == _hashProvider.GetHash(ref _entries.GetRef(i).key) && _entries.GetRef(i).key.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(TKey key, TValue value, bool add)
        {
            ulong targetBucket = _hashProvider.GetHash(ref key) % _buckets.Length;

            for (int i = _buckets.Get(targetBucket); i >= 0; i = _entries.GetRef(i).next)
            {
                if (_hashProvider.GetHash(ref key) == _hashProvider.GetHash(ref _entries.GetRef(i).key) && _entries.GetRef(i).key.Equals(key))
                {
#if !ANOTHERECS_RELEASE
                    if (add)
                    {
                        throw new ArgumentException();
                    }
#endif
                    _entries.GetRef(i).value = value;
                    return;
                }
            }
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

        public bool Remove(TKey key)
        {
            ulong bucket = _hashProvider.GetHash(ref key) % _buckets.Length;
            int last = -1;

            for (int i = _buckets.Get(bucket); i >= 0; last = i, i = _entries.GetRef(i).next)
            {
                ref var entry = ref _entries.GetRef(i);
                if (_hashProvider.GetHash(ref key) == _hashProvider.GetHash(ref _entries.GetRef(i).key) && entry.key.Equals(key))
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

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = _entries.GetRef(i).value;
                return true;
            }
            value = default;
            return false;
        }

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
    }
}