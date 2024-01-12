using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NDictionary<TAllocator, TKey, TValue, THashProvider> : INative, ISerialize, IEnumerable<NDictionary<TAllocator, TKey, TValue, THashProvider>.Pair>, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        where THashProvider : struct, IHash<TKey, uint>
    {
        private NArray<TAllocator, int> _buckets;
        private NArray<TAllocator, Entry> _entries;

        private uint _count;
        private uint _freeCount;
        private int _freeList;

        private THashProvider _hashProvider;

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

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buckets.IsValid && _entries.IsValid;
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
                    return _entries.ReadRef(i).value;
                }
#if ANOTHERECS_RELEASE
                return default;
#else
                throw new KeyNotFoundException();
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
                if (_entries.ReadRef(i).hashCode != 0 && _entries.ReadRef(i).value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindEntry(TKey key)
        {
            for (int i = _buckets.Get(_hashProvider.GetHash(ref key) % _buckets.Length); i >= 0; i = _entries.ReadRef(i).next)
            {
                if (_hashProvider.GetHash(ref key) == _entries.ReadRef(i).hashCode && _entries.ReadRef(i).key.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(TKey key, TValue value, bool add)
        {
            var hashCode = _hashProvider.GetHash(ref key);
            ulong targetBucket = hashCode % _buckets.Length;

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
                    ulong bucket = entry.hashCode % size;
                    entry.next = newBuckets.Read(bucket);
                    newBuckets.ReadRef(bucket) = i;
                }
            }

            _buckets = newBuckets;
        }

        public bool Remove(TKey key)
        {
            ulong hashcode = _hashProvider.GetHash(ref key);
            ulong bucket = hashcode % _buckets.Length;
            int last = -1;

            _entries.Dirty();
            _buckets.Dirty();

            for (int i = _buckets.Read(bucket); i >= 0; last = i, i = _entries.ReadRef(i).next)
            {
                ref var entry = ref _entries.ReadRef(i);
                if (hashcode == _entries.ReadRef(i).hashCode && entry.key.Equals(key))
                {
                    if (last < 0)
                    {
                        _buckets.ReadRef(bucket) = entry.next;
                    }
                    else
                    {
                        _entries.ReadRef(last).next = entry.next;
                    }
                    entry.hashCode = 0;
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
                value = _entries.ReadRef(i).value;
                return true;
            }
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<Pair> GetEnumerator()
            => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

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
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _buckets, ref rebinder);
            MemoryRebinderCaller.Rebind(ref _entries, ref rebinder);
        }

        private struct Entry
        {
            public ulong hashCode;
            public int next;            // Index of next entry, -1 if last
            public TKey key;            // Key of entry
            public TValue value;        // Value of entry
        }

        public struct Pair
        {
            public TKey key;
            public TValue value;
        }


        public struct Enumerator : IEnumerator<Pair>, IEnumerator
        {
            private NDictionary<TAllocator, TKey, TValue, THashProvider> _data;
            private uint _index;
            private Pair _current;

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
                    if (_data._entries.ReadRef(_index).hashCode >= 0)
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

            public Pair Current
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
}