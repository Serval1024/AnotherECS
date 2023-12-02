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
    public unsafe struct NHashSet<TKey, THashProvider> : ISerialize, IDisposable, IEnumerable<TKey>
        where TKey : unmanaged, IEquatable<TKey>
        where THashProvider : struct, IHash<TKey, uint>
    {
        public NArray<int> _buckets;
        private NArray<Slot> _slots;
        private uint _count;
        private int _lastIndex;
        private int _freeList;
        private THashProvider _hashProvider;

        public uint Count
            => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSet(NArray<TKey> list)
        {
            this = new NHashSet<TKey, THashProvider>(list.Length);
            foreach(var element in list)
            {
                Add(element);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSet(uint capacity)
        {
            _lastIndex = 0;
            _count = 0;
            _freeList = -1;

            var size = HashHelpers.GetPrime(capacity);
            _buckets = new NArray<int>(size);
            _slots = new NArray<Slot>(size);
            _hashProvider = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TKey item)
        {            
            for (int i = _buckets.Get(_hashProvider.GetHash(ref item) % _buckets.Length) - 1; i >= 0; i = _slots.GetRef(i).next)
            {
                if (_slots.GetRef(i).item.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey item)
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new ArgumentException();
            }
#endif
            uint bucketId = _hashProvider.GetHash(ref item) % _buckets.Length;

            int index;
            if (_freeList >= 0)
            {
                index = _freeList;
                _freeList = _slots.GetRef(index).next;
            }
            else
            {
                if (_lastIndex == _slots.Length)
                {
                    IncreaseCapacity();
                    bucketId = _hashProvider.GetHash(ref item) % _buckets.Length;
                }
                index = _lastIndex++;
            }
            ref var slot = ref _slots.GetRef(index);
            ref var bucket = ref _buckets.GetRef(bucketId);
            slot.item = item;
            slot.next = bucket - 1;
            bucket = index + 1;
            ++_count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey item)
        {
            uint bucketId = _hashProvider.GetHash(ref item) % _buckets.Length;
            int lastId = -1;
            for (int i = _buckets.Get(bucketId) - 1; i >= 0; lastId = i, i = _slots.GetRef(i).next)
            {
                if (_slots.GetRef(i).item.Equals(item))
                {
                    if (lastId < 0)
                    {
                        _buckets.GetRef(bucketId) = _slots.GetRef(i).next + 1;
                    }
                    else
                    {
                        _slots.GetRef(lastId).next = _slots.GetRef(i).next;
                    }
                    ref var slot = ref _slots.GetRef(i);
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
                _slots.Clear((uint)_lastIndex);

                _lastIndex = 0;
                _count = 0;
                _freeList = -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _buckets.Pack(ref writer);
            _slots.Pack(ref writer);    //_lastIndex == len

            writer.Write(_count);
            writer.Write(_lastIndex);
            writer.Write(_freeList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _buckets.Unpack(ref reader);
            _slots.Unpack(ref reader);

            _count = reader.ReadUInt32();
            _lastIndex = reader.ReadInt32();
            _freeList = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseCapacity()
        {
            uint newSize = HashHelpers.GetPrime(_count << 1);
#if !ANOTHERECS_RELEASE
            if (newSize <= _count)
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
            _buckets.Dispose();

            var newBuckets = new NArray<int>(newSize);
            for (int i = 0; i < _lastIndex; i++)
            {
                var slot = _slots.GetRef(i);
                uint bucket = _hashProvider.GetHash(ref slot.item) % newSize;
                slot.next = newBuckets.GetRef(bucket) - 1;
                newBuckets.GetRef(bucket) = i + 1;
            }

            _buckets = newBuckets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _buckets.Dispose();
            _slots.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(ref this);

        private struct Slot
        {
            public int next;      // Index of next entry, -1 if last
            public TKey item;
        }

        
        public struct Enumerator : IEnumerator<TKey>, IEnumerator
        {
            private NHashSet<TKey, THashProvider> _data;
            private int _index;
            private TKey _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref NHashSet<TKey, THashProvider> data)
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
                while (_index < _data._lastIndex)
                {
                    if (!_data._slots.GetRef(_index).item.Equals(default))
                    {
                        _current = _data._slots.GetRef(_index).item;
                        ++_index;
                        return true;
                    }
                    ++_index;
                }
                return false;
            }

            public TKey Current
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