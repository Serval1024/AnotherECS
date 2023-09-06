using AnotherECS.Core;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe class DArrayStorage : IDisposable, ISerializeConstructor, IRevertSetRecycledCountRaw<ushort>, IRevertGetRecycledRaw<ushort>, IRevertSetCountRaw<ushort>
    {
        private Container[] _dense;
        private ushort[] _recycled;
        private IndexData _data;

#if !ANOTHERECS_HISTORY_DISABLE
        private DArrayHistory _history;
#endif
        
        internal DArrayStorage(ref ReaderContextSerializer reader, in DArrayArgs args)
        {
            Unpack(ref reader, args);
        }

        internal DArrayStorage(in DArrayArgs args)
        {
            _dense = new Container[args.dArrayCapacity + 1];
            _recycled = new ushort[32];

            _data = new IndexData()
            {
                index = 1
            };
#if !ANOTHERECS_HISTORY_DISABLE
            _history = new DArrayHistory(args.history, args.dArrayBuffersCapacity);
#endif
            Init();
        }

        internal void Init()
        {
#if !ANOTHERECS_HISTORY_DISABLE
            _history.SubjectResized(_dense.Length);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Add<T>(int count)
            where T : unmanaged
        {
            ushort denseIndex;

            ref var recycledCount = ref _data.recycle;
            if (recycledCount > 0)
            {
#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushRecycledCount(recycledCount);
#endif
                denseIndex = _recycled[--recycledCount];

                _dense[denseIndex].Prepare<T>(count);
            }
            else
            {
                TryResizeDense();

                ref var currentIndex = ref _data.index;

#if ANOTHERECS_DEBUG
                if (currentIndex == ushort.MaxValue)
                {
                    throw new Exceptions.ReachedLimitComponentException(ushort.MaxValue);
                }
#endif
                denseIndex = currentIndex;
                _dense[currentIndex].Allocate<T>(count);

#if !ANOTHERECS_HISTORY_DISABLE
                _history.PushCount(currentIndex);
#endif
                ++currentIndex;
            }

            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ushort id)
        {
            TryRecycledResized();

            IncVersion(id);
            ref var recycledCount = ref _data.recycle;
#if !ANOTHERECS_HISTORY_DISABLE
            _history.PushRecycledCount(recycledCount);
            _history.PushRecycled(_recycled[recycledCount], recycledCount);
#endif
            _recycled[recycledCount++] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLength(ushort id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return _dense[id].count;
        }

        public void Clear(ushort id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            _dense[id].Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Read(ushort id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return _dense[id].array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(ushort id, int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref _dense[id].Read<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(ushort id, int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref _dense[id].Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw<T>(ushort id, int index, ref T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            _dense[id].SetRaw(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ushort id, int index, ref T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            _dense[id].Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveRigth(ushort id, int index, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            ref var dense = ref _dense[id];
            var elementSize = dense.elementSize;
            var array = (byte*)_dense[id].array;

            for (int i = (count * dense.elementSize) - 1, iMax = (index + 1) * dense.elementSize; i >= iMax; --i)
            {
                array[i] = array[i - elementSize];
            }

            IncVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveLeft(ushort id, int index, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            ref var dense = ref _dense[id];
            var elementSize = dense.elementSize;
            var array = (byte*)_dense[id].array;

            for (int i = index * dense.elementSize, iMax = (count - 1) * dense.elementSize; i < iMax; ++i)
            {
                array[i] = array[i + elementSize];
            }

            IncVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(ushort sourceId, ushort destinationId, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(sourceId);
            ThrowIfOutOfRangeId(destinationId);
#endif
            ref var source = ref _dense[sourceId];
            ref var destination = ref _dense[destinationId];

#if ANOTHERECS_DEBUG
            if (source.elementSize != destination.elementSize)
            {
                throw new ArgumentException("It is not safe to copy to storage with a different data type.");
            }
#endif
            UnsafeMemory.MemCpy(destination.array, source.array, count * source.elementSize);

            IncVersion(destinationId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncVersion(ushort id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            _dense[id].IncVersion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetVersion(ushort id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return _dense[id].version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
#if !ANOTHERECS_HISTORY_DISABLE
            for (ushort i = 0, iMax = _data.index; i < iMax; ++i)
            {
                ref var dense = ref _dense[i];
                if (dense.IsChange())
                {
                    dense.DropChange();
                    _history.PushMemory(i, dense.array, dense.count, dense.elementSize);
                }
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
            => DropChange();

        public void Dispose()
        {
            for(int i = 0; i < _data.index; ++i)
            {
                _dense[i].Deallocate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResizeDense()
        {
            if (_data.index == _dense.Length)
            {
                Array.Resize(ref _dense, _data.index << 1);
#if !ANOTHERECS_HISTORY_DISABLE
                _history.SubjectResized(_dense.Length);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryRecycledResized()
        {
            if (_data.recycle == _recycled.Length)
            {
                Array.Resize(ref _recycled, _data.recycle << 1);
            }
        }

        private void ThrowIfOutOfRangeId(int id)
        {
            if (id == 0)
            {
                throw new IndexOutOfRangeException($"Call Allocate() before use access methods.");
            }
            else if (id < 1 || id >= _dense.Length)
            {
                throw new IndexOutOfRangeException($"Id {id} is out of range Length {_dense.Length}.");
            }
        }

        private void ThrowIfOutOfRangeIndex(ushort id, int index)
        {
            ThrowIfOutOfRangeId(id);

            if (index < 0 || index >= GetLength(id))
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range Length {GetLength(id)}.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DropChange()
        {
            for (int i = 0; i < _data.index; ++i)
            {
                _dense[i].DropChange();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRevertSetRecycledCountRaw<ushort>.SetRecycledCountRaw(ushort value)
        {
            _data.recycle = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ushort[] IRevertGetRecycledRaw<ushort>.GetRecycledRaw()
            => _recycled;

        void IRevertSetCountRaw<ushort>.SetCountRaw(ushort value)
        {
            _data.index = value;
        }

        internal void SetElementCountRaw(ushort id, int size, int elementSize)
        {
            if (size * elementSize > _dense[id].ByteLength)
            {
                _dense[id].Resize(size, elementSize);
            }
        }

        internal Container[] GetDenseRaw()
            => _dense;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(_dense);
            writer.Pack(_recycled);
            writer.Pack(_data);
#if !ANOTHERECS_HISTORY_DISABLE
            writer.Pack(_history);
#endif
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader, default);
        }

        public void Unpack(ref ReaderContextSerializer reader, in DArrayArgs args)
        {
            _dense = reader.Unpack<Container[]>();
            _recycled = reader.Unpack<ushort[]>();
            _data = reader.Unpack<IndexData>();
#if !ANOTHERECS_HISTORY_DISABLE
            _history = reader.Unpack<DArrayHistory>(args);
#endif
        }

        internal unsafe struct Container : ISerialize
        {
            public int count;
            public int elementSize;
            public void* array;

            public int lastVersion;
            public int version;

            public int ByteLength
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => count * elementSize;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsValide()
                => count != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsChange()
                => lastVersion != version;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DropChange()
            {
                lastVersion = version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetRaw<T>(int index, ref T value)
                where T : unmanaged
            {
                UnsafeMemory.GetElementArray<T>(array, index) = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set<T>(int index, ref T value)
                where T : unmanaged
            {
                IncVersion();
                SetRaw(index, ref value);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T Get<T>(int index)
                where T : unmanaged
            {
                IncVersion();
                return ref UnsafeMemory.GetElementArray<T>(array, index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T Read<T>(int index)
                where T : unmanaged
                => ref UnsafeMemory.GetElementArray<T>(array, index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Allocate<T>(int size)
                where T : unmanaged
            {
                Allocate(size, sizeof(T));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Allocate(int size, int elementSize)
            {
                if (!IsValide())
                {
                    this.elementSize = elementSize;
                    count = size;
                    if (size > 0)
                    {
                        array = AllocateInternal(size, elementSize);

                        IncVersion();
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Prepare<T>(int sizeMin)
                where T : unmanaged
            {
                if (count < sizeMin || count > (sizeMin >> 1))
                {
                    Deallocate();
                    count = sizeMin;
                    elementSize = sizeof(T);
                    array = AllocateInternal(sizeMin, elementSize);
                }
                else
                {
                    Clear();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Resize(int size, int elementSize)
            {
                Deallocate();
                Allocate(size, elementSize);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Deallocate()
            {
                if (IsValide())
                {
                    UnsafeMemory.Deallocate(array);
                    count = 0;
                    elementSize = 0;

                    IncVersion();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Replace(void* newArray, int newCount, int newElementSize)
            {
                if (IsValide())
                {
                    UnsafeMemory.Free(array);
                }
                array = newArray;
                count = newCount;
                elementSize = newElementSize;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void IncVersion()
            {
                version = unchecked(version + 1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                if (IsValide())
                {
                    UnsafeMemory.MemClear(array, count * elementSize);

                    IncVersion();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void* AllocateInternal(int size, int sizeElement)
                => UnsafeMemory.Allocate(size * sizeElement);

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(count);
                writer.Write(elementSize);
                writer.WriteStruct(new ArrayPtr(array, (uint)ByteLength));
                writer.Write(lastVersion);
                writer.Write(version);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                count = reader.ReadInt32();
                elementSize = reader.ReadInt32();
                array = reader.ReadStruct<ArrayPtr>().data;
                lastVersion = reader.ReadInt32();
                version = reader.ReadInt32();
            }
        }


        private struct IndexData : ISerialize
        {
            public ushort recycle;
            public ushort index;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(recycle);
                writer.Write(index);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                recycle = reader.ReadUInt16();
                index = reader.ReadUInt16();
            }
        }
    }
}
