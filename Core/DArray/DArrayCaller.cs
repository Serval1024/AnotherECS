using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using static AnotherECS.Core.Caller.FFF;


/*
using ImplCaller = Caller<
               ushort, AnotherECS.Core.DArrayContainer, ushort, AnotherECS.Core.TickOffsetData<AnotherECS.Core.DArrayContainer>, AnotherECS.Core.DArrayContainer,
               InjectFeature<ushort, TEST, ushort, TickOffsetData<TEST>>,
               RecycleStorageFeature<ushort, TEST, ushort, TickOffsetData<TEST>, TEST, UshortNextNumber>,
               DefaultFeature<TEST>,
               AttachDetachFeature<ushort>,
               AttachFeature<ushort, TEST, ushort, TickOffsetData<TEST>>,
               DetachFeature<ushort, TEST, ushort, TickOffsetData<TEST>>,
               UshortSparseFeature<TEST, TickOffsetData<TEST>, TEST>,
               UshortDenseFeature<ushort, TEST, TickOffsetData<TEST>>,
               TrueConst,
               CopyableFeature<TEST>,
               UshortVersionFeature<ushort, TEST, TickOffsetData<TEST>>,
               ByChangeHistoryFeature<ushort, TEST, ushort>,
               BBSerialize<ushort, TEST, ushort, TickOffsetData<TEST>>
               >;
*/
namespace AnotherECS.Core
{
    using DArrayLayout =
        UnmanagedLayout<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>;
    
    using ImplCaller = Caller<
               ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>, DArrayContainer,
               Nothing<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>,
               RecycleStorageFeature<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>, DArrayContainer, UshortNextNumber>,
               DefaultFeature<DArrayContainer>,
               Nothing<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>,
               Nothing<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>,
               Nothing<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>,
               UshortSparseFeature<DArrayContainer, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
               UshortDenseFeature<ushort, DArrayContainer, TickIndexerOffsetData<DArrayContainer>>,
               Nothing,
               CopyableFeature<DArrayContainer>,
               UshortVersionFeature<ushort, DArrayContainer, TickIndexerOffsetData<DArrayContainer>>,
               ByVersionHistoryFeature<ushort, DArrayContainer, ushort>,
               SSSerialize<ushort, DArrayContainer, ushort, TickIndexerOffsetData<DArrayContainer>>
               >;

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct DArrayCaller : ICaller<DArrayContainer>, IDisposable, ISerialize
    {
        //private DArrayLayout* _layout;
        //private GlobalDepencies* _depencies;



        public bool IsValide
            => _layout != null && _depencies != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            Config((UnmanagedLayout<DArrayContainer>*)layout, depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Config(UnmanagedLayout<DArrayContainer>* layout, GlobalDepencies* depencies)
        {
            _layout = layout;
            _depencies = depencies;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            DArrayActions.AllocateLayout(ref *_layout, ref *_depencies);
        }

        DArrayContainer ICaller<DArrayContainer>.Create()
            => default;

        public Type GetElementType()
            => typeof(DArrayContainer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add<T>(int count)
            where T : unmanaged
        {
            MultiStorageActions<DArrayContainer>.TryResizeDense(ref *_layout);

            var denseIndex = MultiStorageActions<DArrayContainer>.AllocateIdHistory(ref *_layout, ref *_depencies);
            ref var container = ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, denseIndex);
            container.Prepare<T>((uint)count);
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            MultiStorageActions<DArrayContainer>.TryResizeRecycle(ref *_layout);
            IncVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLength(uint id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).count;
        }

        public void Clear(uint id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Read(uint id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).data.GetPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(uint id, int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).Read<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint id, int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw<T>(uint id, int index, ref T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).SetRaw(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint id, int index, ref T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
#endif
            MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveRigth(uint id, int index, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            if (count == 0)
            {
                return;
            }

            ref var dense = ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id);
            var elementSize = dense.data.ElementSize;
            var array = (byte*)dense.data.GetPtr();

            for (int i = (count * (int)dense.data.ElementSize) - 1, iMax = (index + 1) * (int)dense.data.ElementSize; i >= iMax; --i)
            {
                array[i] = array[i - elementSize];
            }

            IncVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveLeft(uint id, int index, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            if (count == 0)
            {
                return;
            }

            ref var dense = ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id);
            var elementSize = dense.data.ElementSize;
            var array = (byte*)dense.data.GetPtr();

            for (int i = index * (int)dense.data.ElementSize, iMax = (count - 1) * (int)dense.data.ElementSize; i < iMax; ++i)
            {
                array[i] = array[i + elementSize];
            }

            IncVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(uint sourceId, uint destinationId, int count)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(sourceId);
            ThrowIfOutOfRangeId(destinationId);
#endif
            ref var source = ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, sourceId);
            ref var destination = ref MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, destinationId);

            destination.data.CopyFrom(source.data, (uint)count);

            IncVersion(destinationId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncVersion(uint id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).IncVersion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetVersion(uint id)
        {
#if ANOTHERECS_DEBUG
            ThrowIfOutOfRangeId(id);
#endif
            return MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, id).version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
#if !ANOTHERECS_HISTORY_DISABLE
            var dense = _layout->storage.dense.GetPtr<DArrayContainer>();
            for (uint i = 0, iMax = _layout->storage.denseIndex; i < iMax; ++i)
            {
                if (dense[i].IsChange)
                {
                    dense[i].DropChange();
                    CopyableHistoryFacadeActions<DArrayContainer>.PushDense(ref *_layout, ref *_depencies, i, ref dense[i]);
                }
            }
#endif
        }

#if !ANOTHERECS_HISTORY_DISABLE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            MultiHistoryFacadeActions<DArrayContainer>.RevertToRecycleCountBuffer(ref *_layout, tick);
            MultiHistoryFacadeActions<DArrayContainer>.RevertToRecycleBuffer(ref *_layout, tick);

            MultiHistoryFacadeActions<DArrayContainer>.RevertToCountBuffer(ref *_layout, tick);
            MultiHistoryFacadeActions<DArrayContainer>.RevertToDenseBuffer(ref *_layout, tick);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
#if !ANOTHERECS_HISTORY_DISABLE
            DropChange();
#endif
        }

        public void Dispose()
        {
            for (uint i = 0, iMax = _layout->storage.denseIndex; i < iMax; ++i)
            {
                MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, i).Dispose();
            }
        }

        private void ThrowIfOutOfRangeId(uint id)
        {
            if (id == 0)
            {
                throw new IndexOutOfRangeException($"Call Allocate() before use access methods.");
            }
            else if (id < 1 || id >= _layout->storage.dense.ElementCount)
            {
                throw new IndexOutOfRangeException($"Id {id} is out of range Length {_layout->storage.dense.ElementCount}.");
            }
        }

        private void ThrowIfOutOfRangeIndex(uint id, int index)
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
            for (uint i = 0, iMax = _layout->storage.denseIndex; i < iMax; ++i)
            {
                MultiStorageActions<DArrayContainer>.ReadDirect(ref *_layout, i).DropChange();
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            CustomSerializeActions<DArrayContainer>.Pack(ref writer, ref *_layout, HistoryMode.BYCHANGE, _layout->storage.denseIndex);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            CustomSerializeActions<DArrayContainer>.Unpack(ref reader, ref *_layout, HistoryMode.BYCHANGE);
        }

       
    }

    [IgnoreCompile]
    internal unsafe struct DArrayContainer : ICopyable<DArrayContainer>, IDisposable, ISerialize, IDefault
    {
        public ArrayPtr data;
        public int count;

        public int lastVersion;
        public int version;

        public int ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)data.ByteLength;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data.IsValide;
        }

        public bool IsChange
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => lastVersion != version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropChange()
        {
            lastVersion = version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw<T>(int index, ref T value)
            where T : unmanaged
        {
            data.Set(index, value);
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
            return ref data.GetRef<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(int index)
            where T : unmanaged
            => ref data.GetRef<T>(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate<T>(uint size)
            where T : unmanaged
        {
            Allocate(size, (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint size, uint elementSize)
        {
            if (!IsValide)
            {
                if (size > 0)
                {
                    data = new ArrayPtr(elementSize * size, size);
                    count = (int)size;
                    IncVersion();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepare<T>(uint sizeMin)
            where T : unmanaged
        {
            if (!data.IsValide)
            {
                Allocate<T>(sizeMin);
            }
            else if (data.ElementCount < sizeMin || data.ElementCount > (sizeMin >> 1))
            {
                Resize(sizeMin, data.ElementSize);
            }
            else
            {
                Clear();
            }
            count = (int)sizeMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint size, uint elementSize)
        {
            data.Resize(size, elementSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            if (IsValide)
            {
                data.Dispose();
                IncVersion();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(ArrayPtr data)
        {
            if (IsValide)
            {
                Deallocate();
            }
            this.data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncVersion()
        {
            version = unchecked(version + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (IsValide)
            {
                data.Clear();

                IncVersion();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            data.Dispose();
        }

        public void CopyFrom(in DArrayContainer other)
        {
            data.CopyFrom(other.data);
            count = other.count;
            lastVersion = other.lastVersion;
            version = other.version;
        }

        public void OnRecycle()
        {
            data.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(count);
            writer.WriteStruct(data);
            writer.Write(lastVersion);
            writer.Write(version);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            count = reader.ReadInt32();
            data = reader.ReadStruct<ArrayPtr>();
            lastVersion = reader.ReadInt32();
            version = reader.ReadInt32();
        }
    }
}
