using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct NHashSetUintId : ISerialize, IManualRevert<uint>, IDisposable, IEnumerable<uint>
    {
        private Data _data;

        public bool IsValide
            => _data.IsValide;

        public uint Count
            => *_data.GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSetUintId(uint capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            _data = new Data(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(uint item)
        {
            for (uint i = _data.GetBucket(item % _data.GetBucketLength()) - 1; i != uint.MaxValue; i = _data.GetSlot(i).next)
            {
                if (_data.GetSlot(i).item.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<TManualHistory>(uint item, ref TManualHistory history, uint historyId)
            where TManualHistory : IManualHistoryCaller<uint>
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new ArgumentException();
            }
#endif
            uint bucketId = item % _data.GetBucketLength();

            uint index;
            if (*_data.GetFreeList() != uint.MaxValue)
            {
                index = *_data.GetFreeList();

                history.DirectPush(historyId, Data.HEADER_OFFSET_FREELIST, _data.GetFreeList());
                *_data.GetFreeList() = _data.GetSlot(index).next;
            }
            else
            {
                if (*_data.GetLastIndex() == _data.GetSlotLength())
                {
                    IncreaseCapacity(ref history, historyId); 
                    bucketId = item % _data.GetBucketLength();
                }

                history.DirectPush(historyId, Data.HEADER_OFFSET_LASTINDEX, _data.GetLastIndex());
                index = (*_data.GetLastIndex())++;
            }
            var slotOffset = _data.GetSlotOffset(index);
            var slot = (Slot*)_data.GetPtr(slotOffset);

            var bucketOffset = _data.GetBucketOffset(bucketId);
            var bucket = _data.GetPtr(bucketOffset);

            history.DirectPush(historyId, bucketOffset, bucket);
            history.DirectPush(historyId, slotOffset, &slot->item);
            history.DirectPush(historyId, slotOffset + 1, &slot->next);

            slot->next = *bucket - 1;
            slot->item = item;
            *bucket = index + 1;

            history.DirectPush(historyId, Data.HEADER_OFFSET_COUNT, _data.GetCount());
            ++*_data.GetCount();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<TManualHistory>(uint item, ref TManualHistory history, uint historyId)
            where TManualHistory : IManualHistoryCaller<uint>
        {
            uint bucketId = item % _data.GetBucketLength();
            uint lastId = uint.MaxValue;

            for (uint i = _data.GetBucket(bucketId) - 1; i != uint.MaxValue; lastId = i, i = _data.GetSlot(i).next)
            {
                if (_data.GetSlot(i).item.Equals(item))
                {
                    if (lastId == uint.MaxValue)
                    {
                        var bucketOffset = _data.GetBucketOffset(bucketId);
                        var bucket = _data.GetPtr(bucketOffset);
                        history.DirectPush(historyId, bucketOffset, bucket);

                        *bucket = _data.GetSlot(i).next + 1;
                    }
                    else
                    {
                        var slotOffset = _data.GetSlotOffset(i);
                        var slot = (Slot*)_data.GetPtr(slotOffset);

                        history.DirectPush(historyId, slotOffset, &slot->next);
                        slot->next = _data.GetSlot(i).next;
                    }
                    {
                        var slotOffset = _data.GetSlotOffset(i);
                        var slot = (Slot*)_data.GetPtr(slotOffset);

                        history.DirectPush(historyId, slotOffset, &slot->next);
                        history.DirectPush(historyId, slotOffset, &slot->item);

                        slot->next = *_data.GetFreeList();
                        slot->item = default;

                        if (--*_data.GetCount() == 0)
                        {
                            history.DirectPush(historyId, Data.HEADER_OFFSET_LASTINDEX, _data.GetLastIndex());
                            *_data.GetLastIndex() = 0;

                            history.DirectPush(historyId, Data.HEADER_OFFSET_FREELIST, _data.GetFreeList());
                            *_data.GetFreeList() = uint.MaxValue;
                        }
                        else
                        {
                            history.DirectPush(historyId, Data.HEADER_OFFSET_FREELIST, _data.GetFreeList());
                            *_data.GetFreeList() = i;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseCapacity<TManualHistory>(ref TManualHistory history, uint historyId)
            where TManualHistory : IManualHistoryCaller<uint>
        {
            if (_data.GetBucketLength() > _data.GetSlotLength())
            {
                SetCapacity(*_data.GetCount(), ref history, historyId); //after rollback can disbalanced.
            }
            else
            {
                uint newSize = HashHelpers.GetPrime(*_data.GetCount() << 1);

#if !ANOTHERECS_RELEASE
            if (newSize <= *_data.GetCount())
            {
                throw new ArgumentException();
            }
#endif
                SetCapacity(newSize, ref history, historyId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity<TManualHistory>(uint newSize, ref TManualHistory history, uint historyId)
            where TManualHistory : IManualHistoryCaller<uint>
        {
            _data.SetCapacity(newSize, ref history, historyId);
            
            for (uint i = 0, iMax = *_data.GetLastIndex(); i < iMax; ++i)
            {
                var slotOffset = _data.GetSlotOffset(i);
                var slot = (Slot*)_data.GetPtr(slotOffset);

                uint bucketId = slot->item % newSize;

                var bucketOffset = _data.GetBucketOffset(bucketId);
                var bucket = _data.GetPtr(bucketOffset);

                history.DirectPush(historyId, slotOffset, &slot->next);
                slot->next = *bucket - 1;

                history.DirectPush(historyId, bucketOffset, bucket);
                *bucket = i + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
            => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRevert(uint index, uint segment)
        {
            _data.OnRevert(index, segment);
        }

        [StructLayout(LayoutKind.Sequential, Size = 8)]
        private struct Slot
        {
            public uint next;      // Index of next entry, uint.MaxValue if last
            public uint item;
        }

        private struct Data : IManualRevert<uint>, ISerialize, IDisposable
        {
            public const uint HEADER_OFFSET_COUNT = 0;
            public const uint HEADER_OFFSET_LASTINDEX = 1;
            public const uint HEADER_OFFSET_FREELIST = 2;
            public const uint HEADER_OFFSET_OFFSETBUCKET = 3;
            public const uint HEADER_SIZE_IN_UINT = 4;

            private NArray<uint> _data;

            public bool IsValide
                => _data.IsValide;

            public Data(uint capacity)
            {
                _data = new NArray<uint>((capacity << 1) + capacity + HEADER_SIZE_IN_UINT);
                *GetOffsetBucket() = HEADER_SIZE_IN_UINT + (capacity << 1);
                *GetFreeList() = uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint* GetCount()
                => _data.GetPtr(HEADER_OFFSET_COUNT);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint* GetLastIndex()
                => _data.GetPtr(HEADER_OFFSET_LASTINDEX);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint* GetFreeList()
                => _data.GetPtr(HEADER_OFFSET_FREELIST);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCapacity<TManualHistory>(uint capacity, ref TManualHistory history, uint historyId)
                where TManualHistory : IManualHistoryCaller<uint>
            {
                var arrayLength = (capacity << 1) + capacity + HEADER_SIZE_IN_UINT;

                var newArray = new NArray<uint>(arrayLength);
                var newOffsetBucket = HEADER_SIZE_IN_UINT + (capacity << 1);
                var deltaBucket = newOffsetBucket - *GetOffsetBucket();

                for (uint i = 0; i < *GetOffsetBucket(); ++i)    //slots & meta
                {
                    newArray.GetRef(i) = _data.GetRef(i);
                }

                _data.Dispose();
                _data = newArray;

                history.DirectPush(historyId, HEADER_OFFSET_OFFSETBUCKET, GetOffsetBucket());
                *GetOffsetBucket() = newOffsetBucket;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSlotCapacity(uint capacity)
            {
#if !ANOTHERECS_RELEASE
                if (capacity <= GetSlotLength())
                {
                    throw new ArgumentOutOfRangeException(nameof(capacity));
                }
#endif
                var newArray = new NArray<uint>(_data.Length + (capacity - GetSlotLength()) << 1);
                var newOffsetBucket = _data.Length - GetBucketLength();
                var deltaBucket = newOffsetBucket - *GetOffsetBucket();


                for (uint i = 0; i < *GetOffsetBucket(); ++i)    //slots & meta
                {
                    newArray.GetRef(i) = _data.GetRef(i);
                }

                for (uint i = *GetOffsetBucket(); i < _data.Length; ++i) //buckets
                {
                    newArray.GetRef(i + deltaBucket) = _data.GetRef(i);
                }

                *GetOffsetBucket() = newOffsetBucket;

                _data.Dispose();
                _data = newArray;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetBucketCapacity(uint capacity)
            {
#if !ANOTHERECS_RELEASE
                if (capacity <= GetBucketLength())
                {
                    throw new ArgumentOutOfRangeException(nameof(capacity));
                }
#endif
                _data.Resize(_data.Length + (capacity - GetBucketLength()));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetSlotLength()
               => (*GetOffsetBucket() - HEADER_SIZE_IN_UINT) >> 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetBucketLength()
               => _data.Length - *GetOffsetBucket();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref Slot GetSlot(uint index)
                => ref *(Slot*)_data.GetPtr(GetSlotOffset(index));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref uint GetBucket(uint index)
                => ref _data.GetRef(GetBucketOffset(index));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref uint Get(uint index)
                => ref _data.GetRef(index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint* GetPtr(uint index)
                => _data.GetPtr(index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetSlotOffset(uint index)
                => HEADER_SIZE_IN_UINT + (index << 1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetBucketOffset(uint index)
                => *GetOffsetBucket() + index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _data.Dispose();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                if (*GetLastIndex() > 0)
                {
                    _data.Clear();
                    *GetFreeList() = uint.MaxValue;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Pack(ref WriterContextSerializer writer)
            {
                _data.Pack(ref writer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Unpack(ref ReaderContextSerializer reader)
            {
                _data.Unpack(ref reader);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRevert(uint index, uint segment)
            {
                _data.GetRef(index) = segment;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint* GetOffsetBucket()
                => _data.GetPtr(HEADER_OFFSET_OFFSETBUCKET);
        }


        public struct Enumerator : IEnumerator<uint>, IEnumerator
        {
            private Data _data;
            private uint _index;
            private uint _current;

            public bool IsValide
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.IsValide;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref NHashSetUintId data)
            {
                _data = data._data;
                _index = 0;
                _current = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_index < *_data.GetLastIndex())
                {
                    if (!_data.GetSlot(_index).item.Equals(default))
                    {
                        _current = _data.GetSlot(_index).item;
                        ++_index;
                        return true;
                    }
                    ++_index;
                }
                return false;
            }

            public uint Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == *_data.GetLastIndex() + 1)
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