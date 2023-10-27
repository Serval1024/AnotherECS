using System;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public struct UintSet : ISerialize
    {
        private int[] _buckets;
        private Slot[] _slots;
        private int _count;
        private int _lastIndex;
        private int _freeList;

        public int Count
            => _count;

        public UintSet(uint capacity)
        {
            _lastIndex = 0;
            _count = 0;
            _freeList = -1;

            var size = HashHelpers.GetPrime(capacity);
            _buckets = new int[size];
            _slots = new Slot[size];
        }

        public bool Contains(uint item)
        {            
            for (int i = _buckets[item % _buckets.Length] - 1; i >= 0; i = _slots[i].next)
            {
                if (_slots[i].item == item)
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(uint item)
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new ArgumentException();
            }
#endif
            uint bucketId = item % (uint)_buckets.Length;

            int index;
            if (_freeList >= 0)
            {
                index = _freeList;
                _freeList = _slots[index].next;
            }
            else
            {
                if (_lastIndex == _slots.Length)
                {
                    IncreaseCapacity();
                    bucketId = item % (uint)_buckets.Length;
                }
                index = _lastIndex++;
            }
            ref var slot = ref _slots[index];
            ref var bucket = ref _buckets[bucketId];
            slot.item = item;
            slot.next = bucket - 1;
            bucket = index + 1;
            ++_count;
        }

        public bool Remove(uint item)
        {
            uint bucketId = item % (uint)_buckets.Length;
            int lastId = -1;
            for (int i = _buckets[bucketId] - 1; i >= 0; lastId = i, i = _slots[i].next)
            {
                if (_slots[i].item == item)
                {
                    if (lastId < 0)
                    {
                        _buckets[bucketId] = _slots[i].next + 1;
                    }
                    else
                    {
                        _slots[lastId].next = _slots[i].next;
                    }
                    _slots[i].next = _freeList;
                    
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

        public void Clear()
        {
            if (_lastIndex > 0)
            {
                Array.Clear(_buckets, 0, _buckets.Length);
                Array.Clear(_slots, 0, _lastIndex);

                _lastIndex = 0;
                _count = 0;
                _freeList = -1;
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.WriteUnmanagedArray(_buckets);
            writer.WriteUnmanagedArray(_slots, _lastIndex);

            writer.Write(_count);
            writer.Write(_lastIndex);
            writer.Write(_freeList);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _buckets = reader.ReadUnmanagedArray<int>();
            _slots = reader.ReadUnmanagedArray<Slot>();

            _count = reader.ReadInt32();
            _lastIndex = reader.ReadInt32();
            _freeList = reader.ReadInt32();
        }


        private void IncreaseCapacity()
        {
            uint newSize = HashHelpers.GetPrime(((uint)_count) << 1);
#if !ANOTHERECS_RELEASE
            if (newSize <= _count)
            {
                throw new ArgumentException();
            }
#endif
            SetCapacity(newSize);
        }

        private void SetCapacity(uint newSize)
        {
            Slot[] newSlots = new Slot[newSize];
            Array.Copy(_slots, 0, newSlots, 0, _lastIndex);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < _lastIndex; i++)
            {
                uint bucket = newSlots[i].item % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            _slots = newSlots;
            _buckets = newBuckets;
        }

        private struct Slot
        {
            public int next;      // Index of next entry, -1 if last
            public uint item;
        }

        /*
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private HashSet<T> set;
            private int index;
            private int version;
            private T current;

            internal Enumerator(HashSet<T> set)
            {
                this.set = set;
                index = 0;
                version = set.m_version;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (version != set.m_version)
                {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                }

                while (index < set.m_lastIndex)
                {
                    if (set.m_slots[index].hashCode >= 0)
                    {
                        current = set.m_slots[index].value;
                        index++;
                        return true;
                    }
                    index++;
                }
                index = set.m_lastIndex + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == set.m_lastIndex + 1)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumOpCantHappen));
                    }
                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (version != set.m_version)
                {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                }

                index = 0;
                current = default(T);
            }
        }*/
    }

}