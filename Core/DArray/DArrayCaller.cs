using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    using ImplCaller = Caller<
                uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer,
                UintNumber,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                RecycleStorageFeature<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                NonSparseFeature<DArrayContainer, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>,
                UintDenseFeature<uint, DArrayContainer, TickIndexerOffsetData<DArrayContainer>>,
                Nothing,
                CopyableFeature<DArrayContainer>,
                UintVersionFeature<uint, DArrayContainer, TickIndexerOffsetData<DArrayContainer>>,
                ByVersionHistoryFeature<uint, DArrayContainer, uint>,
                SSSerialize<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>>,
                Nothing<uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>, DArrayContainer>
                >;

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct DArrayCaller : ICaller<DArrayContainer>, IDisposable, ISerialize, IRevertCaller, ITickFinishedCaller, IRevertFinishedCaller
    {
        private ImplCaller _impl;

        public bool IsValide => _impl.IsValide;
        public bool IsSingle => false;
        public bool IsRevert => true;
        public bool IsTickFinished => true;
        public bool IsSerialize => true;
        public bool IsResizable => false;
        public bool IsAttach => false;
        public bool IsDetach => false;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            CallerWrapper.Config<ImplCaller, DArrayContainer>(ref _impl, layout, depencies, id, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            CallerWrapper.AllocateLayout<ImplCaller, DArrayContainer>(ref _impl);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        DArrayContainer ICaller<DArrayContainer>.Create()
            => _impl.Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => _impl.GetElementType();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add<T>(int count)
            where T : unmanaged
        {
            var id = _impl.AllocateForId();
            ref var component = ref _impl.UnsafeDirectRead(id);
            component.Prepare<T>((uint)count);

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            _impl.Remove(id);
            _impl.DirectDenseUpdateVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLength(uint id)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(id);
#endif
            return _impl.Read(id).count;
        }

        public void Clear(uint id)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(id);
#endif
            _impl.Get(id).Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Read(uint id)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(id);
#endif
            return _impl.UnsafeDirectRead(id).data.GetPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(uint id, int index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref _impl.UnsafeDirectRead(id).Read<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint id, int index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
#endif
            return ref _impl.Get(id).Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw<T>(uint id, int index, ref T value)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
#endif
            _impl.UnsafeDirectRead(id).Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint id, int index, ref T value)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
#endif
            _impl.Get(id).Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveRigth(uint id, int index, int count)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            if (count == 0)
            {
                return;
            }

            ref var dense = ref _impl.UnsafeDirectRead(id);
            var elementSize = dense.data.ElementSize;
            var array = (byte*)dense.data.GetPtr();

            for (int i = (count * (int)dense.data.ElementSize) - 1, iMax = (index + 1) * (int)dense.data.ElementSize; i >= iMax; --i)
            {
                array[i] = array[i - elementSize];
            }

            UpdateVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveLeft(uint id, int index, int count)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeIndex(id, index);
            ThrowIfOutOfRangeIndex(id, count - 1);
#endif
            if (count == 0)
            {
                return;
            }

            ref var dense = ref _impl.UnsafeDirectRead(id);
            var elementSize = dense.data.ElementSize;
            var array = (byte*)dense.data.GetPtr();

            for (int i = index * (int)dense.data.ElementSize, iMax = (count - 1) * (int)dense.data.ElementSize; i < iMax; ++i)
            {
                array[i] = array[i + elementSize];
            }

            UpdateVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(uint sourceId, uint destinationId, int count)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(sourceId);
            ThrowIfOutOfRangeId(destinationId);
#endif
            ref var source = ref _impl.UnsafeDirectRead(sourceId);
            ref var destination = ref _impl.UnsafeDirectRead(destinationId);

            destination.data.CopyFrom(source.data, (uint)count);

            UpdateVersion(destinationId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateVersion(uint id)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(id);
#endif
            _impl.DirectDenseUpdateVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(uint id)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfOutOfRangeId(id);
#endif
            return _impl.GetVersion(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            _impl.TickFinished();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            _impl.RevertTo(tick, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
            _impl.RevertFinished();
        }

        public void Dispose()
        {
            _impl.Dispose();
        }

        private void ThrowIfOutOfRangeId(uint id)
        {
            if (id == 0)
            {
                throw new IndexOutOfRangeException($"Call Allocate() before use access methods.");
            }
            else if (id < 1 || id >= _impl.GetCapacity())
            {
                throw new IndexOutOfRangeException($"Id {id} is out of range Length {_impl.GetCapacity()}.");
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
        public void Pack(ref WriterContextSerializer writer)
        {
            _impl.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _impl.Unpack(ref reader);
        }

        public bool IsHas(uint id)
            => _impl.Read(id).IsValide;

        void ICaller<DArrayContainer>.Add(uint id, ref DArrayContainer component)
        {
            throw new NotSupportedException();
        }

        ref DArrayContainer ICaller<DArrayContainer>.Add(uint id)
        {
            throw new NotSupportedException();
        }

        ref readonly DArrayContainer ICaller<DArrayContainer>.Read(uint id)
        {
            throw new NotSupportedException();
        }

        ref DArrayContainer ICaller<DArrayContainer>.Get(uint id)
        {
            throw new NotSupportedException();
        }

        void ICaller<DArrayContainer>.Set(uint id, ref DArrayContainer component)
        {
            throw new NotSupportedException();
        }

        void ICaller<DArrayContainer>.SetOrAdd(uint id, ref DArrayContainer component)
        {
            throw new NotSupportedException();
        }

        public IComponent GetCopy(uint id)
        {
            throw new NotSupportedException();
        }

        public void Set(uint id, IComponent data)
        {
            throw new NotSupportedException();
        }

        public static class LayoutInstaller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static DArrayCaller Install(State state)
                => state.AddLayout<DArrayCaller, uint, DArrayContainer, uint, TickIndexerOffsetData<DArrayContainer>>();
        }
    }

    [IgnoreCompile]
    internal unsafe struct DArrayContainer : ICopyable<DArrayContainer>, IDisposable, ISerialize
    {
        public ArrayPtr data;
        public int count;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(int index, ref T value)
            where T : unmanaged
        {
            data.Set(index, value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index)
            where T : unmanaged
        {
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
        public void Clear()
        {
            if (IsValide)
            {
                data.Clear();
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
        }

        public void OnRecycle()
        {
            data.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(count);
            data.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            count = reader.ReadInt32();
            data.Unpack(ref reader);
        }
    }
}
