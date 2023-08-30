using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct BlockMemoryStorage : IDisposable, ISerialize, IRevertSetRecycledCountRaw<uint>, IRevertGetRecycledRaw<uint>, IRevertSetCountRaw<uint>, IRevertPtrDenseRaw
    {
        private byte* _dense;
        private uint _segmentSize;
        private uint _length;
        private uint _count;
        private uint[] _recycled;
        private IndexData _data;

#if !ANOTHERECS_HISTORY_DISABLE
        private BlockHistory _history;
#endif

#if ANOTHERECS_HISTORY_DISABLE
        public BlockMemoryStorage(uint bufferSize, uint segmentSize, uint recycledCapacity)
#else
        public BlockMemoryStorage(uint bufferSize, uint segmentSize, uint recycledCapacity, in HistoryByChangeArgs args)
#endif
        {
            if (bufferSize % segmentSize != 0)
            {
                throw new ArgumentException($"{nameof(bufferSize)} must be even {nameof(segmentSize)}.");
            }

            _dense = (byte*)UnsafeMemory.Allocate(bufferSize);

            _segmentSize = segmentSize;
            _length = bufferSize;
            _count = bufferSize / segmentSize;
            _recycled = new uint[recycledCapacity];

            _data = new IndexData()
            {
                index = 1,
            };
#if !ANOTHERECS_HISTORY_DISABLE
            _history = new BlockHistory(args);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => _data.index - _data.recycle - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetByteCapacity()
            => _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCountCapacity()
            => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add()
        {
            TryDenseResize();
            return UnsafeAdd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint UnsafeAdd()
        {
            ref var recycledCount = ref _data.recycle;
            if (recycledCount > 0)
            {
#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushRecycledCount(recycledCount);
#endif
                return _recycled[--recycledCount];
            }
            else
            {
                ref var denseIndex = ref _data.index;
#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushCount(denseIndex);
#endif
                return denseIndex++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Read<T>(uint id)
            where T : unmanaged
            => (T*)(_dense + id * _segmentSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change2Byte(ushort* ptr)
        {
#if !ANOTHERECS_HISTORY_DISABLE
            _history.Push((uint)(((byte*)ptr) - _dense), *ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            var offsetPtr = id *_segmentSize;
            ref var component = ref _dense[offsetPtr];
            ref var recycle = ref _data.recycle;
#if !ANOTHERECS_HISTORY_DISABLE
            _history.PushRecycledCount(recycle);
            _history.PushRecycled(_recycled[recycle], recycle);
#endif
            _recycled[recycle++] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDenseIncSize()
            => TryDenseResize(_count + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDenseResize()
            => TryDenseResize(_count << 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDenseResize(uint size)
        {
            if (_data.index == _count)
            {
                var newCount = size;
                var newDensePtr = UnsafeMemory.Allocate(newCount * _segmentSize);
                UnsafeMemory.MemCpy(newDensePtr, _dense, _length);
                UnsafeMemory.Deallocate(_dense);
                _dense = (byte*)newDensePtr;
                _length = newCount * _segmentSize;
                _count = newCount;

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRecycledCountRaw(uint count)
        {
            _data.recycle = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] GetRecycledRaw()
            => _recycled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDenseCountRaw(uint count)
        {
            _data.index = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCountRaw(uint value)
        {
            _data.index = value;
        }

        public byte* GetDenseRaw()
           => _dense;



        public void Dispose()
        {
            UnsafeMemory.Deallocate(_dense);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.WriteStruct(new ArrayPtr(_dense, _length));
            writer.Write(_segmentSize);
            writer.WriteUnmanagedArray(_recycled, (int)_data.recycle);
            _data.Pack(ref writer);
#if !ANOTHERECS_HISTORY_DISABLE
            writer.Pack(_history);
#endif
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader, default);
        }

        public void Unpack(ref ReaderContextSerializer reader, in HistoryByChangeArgs args)
        {
            var arrayPtr = reader.ReadStruct<ArrayPtr>();
            _dense = (byte*)arrayPtr.data;
            _length = arrayPtr.length;
            _segmentSize = reader.ReadUInt32();
            _recycled = reader.ReadUnmanagedArray<uint>();
            _data.Unpack(ref reader);
#if !ANOTHERECS_HISTORY_DISABLE
            _history = reader.Unpack<BlockHistory>(args);
#endif
        }

        private struct IndexData : ISerialize
        {
            public uint index;
            public uint recycle;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(index);
                writer.Write(recycle);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                index = reader.ReadUInt32();
                recycle = reader.ReadUInt32();
            }
        }
    }
}
