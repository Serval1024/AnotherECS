using System;
using System.Runtime.CompilerServices;
using System.Threading;
using AnotherECS.Core.Collection;
using AnotherECS.Core.Threading;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal unsafe struct ConcurrentChangeHistory : IHistory, IDisposable, ISerialize
    {
        private BAllocator* _allocator;

        private bool _isNeedRefreshReference;
        private uint _index;
        private int _ordersIndex;
        private bool _isOrders;
        private NArray<BAllocator, byte> _orders;
        private NArray<BAllocator, int> _lockers;
        private NArray<BAllocator, BucketChangeHistory> _buckets;

        private uint _capacity;
        private uint _recordHistoryLength;
        private uint _parallelMax;

        public uint ParallelMax
        {
            get => _parallelMax;
            set
            {
                _parallelMax = value;

                var bucketCount = (value <= 1)
                    ? value
                    : HashHelpers.GetPrime(value);

                if (bucketCount != _buckets.Length)
                {
                    if (bucketCount < _buckets.Length)
                    {
                        for (uint i = bucketCount; i < _buckets.Length; ++i)
                        {
                            GlobalThreadLockerProvider.DeallocateId(_lockers.GetRef(i));
                            _buckets.GetRef(i).Dispose();
                        }
                        _lockers.Resize(value);
                        _buckets.Resize(bucketCount);
                    }
                    else
                    {
                        var lastLength = _buckets.Length;
                        _lockers.Resize(bucketCount);
                        _buckets.Resize(bucketCount);

                        for (uint i = lastLength; i < _buckets.Length; ++i)
                        {
                            _lockers.GetRef(i) = GlobalThreadLockerProvider.AllocateId();
                            _buckets.GetRef(i) = new BucketChangeHistory(_allocator, _capacity, _recordHistoryLength);
                        }
                    }

                    if (bucketCount <= 1)
                    {
                        _isOrders = false;
                        _orders.Dispose();
                    }
                    else
                    {
                        _isOrders = true;
                        _orders = new NArray<BAllocator, byte>(_allocator, 32);
                        _orders.SetAllByte(byte.MaxValue);
                    }
                }
            }
        }

        public ConcurrentChangeHistory(BAllocator* allocator, uint capacity, uint recordHistoryLength, uint bucketsCount)
        {
            _allocator = allocator;

            _isNeedRefreshReference = false;
            _index = 0;
            _ordersIndex = -1;

            _orders = default;
            _lockers = new NArray<BAllocator, int>(allocator, bucketsCount);
            _buckets = new NArray<BAllocator, BucketChangeHistory>(allocator, bucketsCount);
            _capacity = capacity;
            _recordHistoryLength = recordHistoryLength;
            _parallelMax = 0;
            _isOrders = false;

            ParallelMax = bucketsCount;
        }

        public IHistory Create(BAllocator* allocator, uint historyCapacity, uint recordHistoryLength)
            => new ConcurrentChangeHistory(allocator, historyCapacity, recordHistoryLength, 0);

        public void Push(uint tick, ref MemoryHandle memoryHandle, uint size)
        {
            if (_isOrders)
            {
                var bucketId = (uint)Thread.CurrentThread.ManagedThreadId % _buckets.Length;
                var currentIndex = Interlocked.Increment(ref _ordersIndex);
                
                _orders.GetRef(currentIndex % (int)_orders.Length) = (byte)bucketId;    //TODO SER thre resize?

                lock (GlobalThreadLockerProvider.GetLocker(_lockers.Read(bucketId)))
                {
                    _buckets.GetRef(bucketId).Push(tick, ref memoryHandle, size);
                }
            }
            else
            {
                _buckets.GetRef(0).Push(tick, ref memoryHandle, size);
            }
        }

        public unsafe bool RevertTo(ref HAllocator destination, uint tick)
            => _isOrders
                ? OrderRevertTo(ref destination, tick)
                : _buckets.GetRef(0).RevertTo(ref destination, tick);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe bool OrderRevertTo(ref HAllocator destination, uint tick)
        {
            if (_ordersIndex != -1)
            {
                int currentIndex = _ordersIndex % (int)_orders.Length;

                for (int i = currentIndex; i >= 0; --i)
                {
                    ref var bucketId = ref _orders.GetRef(i);

                    if (bucketId == byte.MaxValue || _buckets.GetRef(bucketId).TryStepBack(ref destination, tick))
                    {
                        return _isNeedRefreshReference;
                    }
                }

                for (int i = (int)_orders.Length - 1; i > currentIndex; --i)
                {
                    ref var bucketId = ref _orders.GetRef(i);

                    if (bucketId == byte.MaxValue || _buckets.GetRef(bucketId).TryStepBack(ref destination, tick))
                    {
                        return _isNeedRefreshReference;
                    }
                }
            }
            return _isNeedRefreshReference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            TryDropRefreshReference();
        }

        public void Dispose()
        {
            for (uint i = 0; i < _lockers.Length; ++i)
            {
                GlobalThreadLockerProvider.DeallocateId(_lockers.GetRef(i));
            }

            _buckets.Dispose();
            _lockers.Dispose();
            _orders.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_allocator->GetId());

            writer.Write(_index);
            writer.Write(_ordersIndex);

            writer.Write(_capacity);
            writer.Write(_recordHistoryLength);
            writer.Write(_parallelMax);

            _orders.PackBlittable(ref writer);
            _buckets.Pack(ref writer);
            _lockers.PackBlittable(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            var allocatorId = reader.ReadUInt32();
            _allocator = reader.GetDepency<WPtr<BAllocator>>(allocatorId).Value;

            _index = reader.ReadUInt32();
            _ordersIndex = reader.ReadInt32();

            _capacity = reader.ReadUInt32();
            _recordHistoryLength = reader.ReadUInt32();
            _parallelMax = reader.ReadUInt32();

            _orders.UnpackBlittable(ref reader);
            _buckets.Unpack(ref reader);

            _lockers.UnpackBlittable(ref reader);
            for (uint i = 0; i < _lockers.Length; ++i)
            {
                _lockers.GetRef(i) = GlobalThreadLockerProvider.AllocateId();
            }
            _isNeedRefreshReference = true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryDropRefreshReference()
        {
            if (_isNeedRefreshReference)
            {
                ++_index;
                if (_index >= _buckets.Length)
                {
                    _index = 0;
                    _isNeedRefreshReference = GetNeedDropRefreshReference();
                }
                _buckets.GetRef(_index).TryDropRefreshReference();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetNeedDropRefreshReference()
        {
            for (uint i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets.GetRef(_index).IsNeedDropRefreshReference())
                {
                    return true;
                }
            }
            return false;
        }
    }
}