using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Actions;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [IgnoreCompile]
    internal unsafe struct Caller
      <
      TSparse,
      TDense,
      TDenseIndex,
      TTickData,
      TTickDataDense,

      TInject,
      TIdAllocator,
      TDefaultSetter,
      TAttachDetachStorage,
      TAttach,
      TDetach,
      TSparseStorage,
      TDenseStorage,
      TIsBindToEntity,
      TCopyable,
      TVersion,
      THistory,
      TSerialize
      >
      : ICaller<TDense>, IDisposable, ISerialize, IRevertCaller, ITickFinishedCaller, IAttachCaller, IDetachCaller, IResizableCaller

        where TSparse : unmanaged
        where TDense : unmanaged, IComponent
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged

        where TInject : struct, IInject<TSparse, TDense, TDenseIndex, TTickData>
        where TIdAllocator : struct, IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TDefaultSetter : struct, IDefaultSetter<TDense>
        
        where TAttachDetachStorage : struct, IData, IAttachDetachProvider<TSparse>, IBoolConst
        where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparseStorage : struct, ISparseProvider<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, IIterator<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TDenseStorage : struct, IStartIndexProvider, IDenseProvider<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TIsBindToEntity : struct, IBoolConst
        where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        where TVersion : struct, IChange<TSparse, TDense, TDenseIndex, TTickData>, IVersion<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TSerialize : struct, ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
    {
        private UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* _layout;
        private GlobalDepencies* _depencies;
        private ushort _elementId;
        private TAttachDetachStorage attachDetachStorage;

        private readonly HubLayoutAllocator<
                   TSparse, TDense, TDenseIndex, TTickData,
                   TSparseStorage,
                   TDenseStorage,
                   TIdAllocator,
                   TVersion,
                   THistory> allocator;

        public bool IRevert
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(THistory).IsRevert;
        }

        public bool IsTickFinished
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(THistory).IsTickFinished;
        }

        public bool IsSerialize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TSerialize).Is;
        }
        public bool IsResizable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TDense).Is;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Type ICaller.GetElementType()
            => typeof(TDense);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            _layout = (UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>*)layout;
            _depencies = depencies;
            _elementId = id;
            attachDetachStorage.Allocate(state, ref *depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            allocator.Allocate(ref *_layout, ref *_depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDense Create()
        {
            TInject inject = default;
            TDense component = default;
            inject.Construct(ref *_layout, ref *_depencies, ref component);
            TDefaultSetter defaultSetter = default;
            defaultSetter.SetupDefault(ref component);

            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            attachDetachStorage.Dispose();
            Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
        {
            TIdAllocator idAllocator = default;
            TDenseStorage dense = default;
            return idAllocator.GetCount(ref *_layout, dense.GetIndex());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
        {
            TDenseStorage dense = default;
            return dense.GetCapacity(ref *_layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint capacity)
        {
            allocator.SparseResize<TSparseStorage>(ref *_layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach()
        {
            TAttach attach = default;
            TDenseStorage dense = default;
            attach.Attach<TSparseStorage>(_layout, attachDetachStorage.GetState(), dense.GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach()
        {
            TDetach detach = default;
            TDenseStorage dense = default;
            detach.Detach<TSparseStorage>(_layout, attachDetachStorage.GetState(), dense.GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject()
        {
            TSparseStorage sparseStorage = default;
            TDenseStorage dense = default;
            sparseStorage.ForEach<ConstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent GetCopy(EntityId id)
            => Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, IComponent data)
        {
            ref var component = ref Get(id);
            component = (TDense)data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
        {
            TSparseStorage sparse = default;
            return sparse.IsHas(ref *_layout, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TDense Read(EntityId id)
        {
            TSparseStorage sparse = default;
            TDenseStorage dense = default;
            return ref dense.GetDense(ref *_layout, sparse.ConvertToDenseIndex(ref *_layout, id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense Get(EntityId id)
        {
            TSparseStorage sparse = default;
            var denseIndex = sparse.ConvertToDenseIndex(ref *_layout, id);

            TDenseStorage dense = default;
            ref var component = ref dense.GetDense(ref *_layout, denseIndex);

            TVersion change = default;
            change.Change(ref *_layout, ref *_depencies, denseIndex);

            THistory history = default;
            history.PushDense<TCopyable>(ref *_layout, ref *_depencies, denseIndex, ref component);

            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, TDense data)
        {
            Set(id, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, ref TDense data)
        {
            ref var component = ref Get(id);
            component = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd(EntityId id, ref TDense component)
        {
            if (IsHas(id))
            {
                Set(id, ref component);
            }
            else
            {
                Add(id, ref component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense Add(EntityId id)
        {
            ref var component = ref AddInternal(id);
            AddPostInternal(id, ref component);
            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense Add(EntityId id, ref TDense data)
        {
            ref var component = ref AddInternal(id);
            component = data;

            AddPostInternal(id, ref component);

            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid(EntityId id)
        {
            AddPostInternal(id, ref AddInternal(id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id)
        {
            TSparseStorage sparseStorage = default;
            THistory history = default;
            TDenseStorage denseStorage = default;

            var denseIndex = sparseStorage.ConvertToDenseIndex(ref *_layout, id);
            ref var sparse = ref sparseStorage.GetSparse(ref *_layout, id);
            history.PushSparse(ref *_layout, ref *_depencies, id, sparse);
            sparse = default;

            ref var component = ref denseStorage.GetDense(ref *_layout, denseIndex);

            history.PushDense<TCopyable>(ref *_layout, ref *_depencies, denseIndex, ref component);

            TCopyable copyable = default;
            copyable.Recycle(ref component);

            TIdAllocator idAllocator = default;
            idAllocator.DeallocateId<THistory>(ref *_layout, ref *_depencies, denseIndex);

            TDetach detach = default;
            TAttachDetachStorage attachDetachStorage = default;
            detach.Detach(attachDetachStorage.GetState(), ref component);

            TInject inject = default;
            inject.Deconstruct(ref *_layout, ref *_depencies, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(EntityId id)
        {
            TVersion version = default;
            return version.GetVersion(ref *_layout, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetStorage()
        {
            Detach();

            TDenseStorage dense = default;
            TSparseStorage sparseStorage = default;

            TCopyable copyable = default;
            if (copyable.Is)
            {
                copyable.Recycle<TSparse, TDenseIndex, TTickData, TTickDataDense, TSparseStorage>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
            }
            sparseStorage.ForEach<DeconstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());

            StorageActions.StorageClear(ref *_layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetHistory()
        {
            THistory history = default;
            history.HistoryClear<TCopyable>(ref *_layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            ResetStorage();
            ResetHistory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            THistory history = default;
            history.TickFinished<TCopyable>(ref *_layout, ref *_depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            THistory history = default;
            history.RevertTo<TAttachDetachStorage, TAttach, TDetach, TSparseStorage, TVersion>
                (_layout, ref attachDetachStorage, tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            TSerialize serialize = default;
            serialize.Pack(ref writer, _layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            TSerialize serialize = default;
            serialize.Unpack(ref reader, _layout);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref TDense AddInternal(EntityId id)
        {
            TIsBindToEntity isBindToEntity = default;
            if (isBindToEntity.Is)
            {
                _depencies->entities.Add(id, _elementId);
            }

            TryResizeDense();
            TIdAllocator idAllocator = default;
            var denseIndex = idAllocator.AllocateId<THistory>(ref *_layout, ref *_depencies);

            TVersion version = default;
            version.Change(ref *_layout, ref *_depencies, denseIndex);

            TSparseStorage sparseStorage = default;
            sparseStorage.SetSparse<THistory>(ref *_layout, ref *_depencies, id, denseIndex);
            TDenseStorage senseStorage = default;
            ref var component = ref senseStorage.GetDense(ref *_layout, denseIndex);

            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPostInternal(EntityId id, ref TDense component)
        {
            TInject inject = default;
            inject.Construct(ref *_layout, ref *_depencies, ref component);
            TAttach attach = default;
            attach.Attach(attachDetachStorage.GetState(), ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResizeDense()
        {
            TDenseStorage denseStorage = default;
            uint capacity = denseStorage.GetCapacity(ref *_layout);
            if (denseStorage.GetAllocated(ref *_layout) == capacity)
            {
                allocator.DenseResize(ref *_layout, capacity << 1);
            }
        }
    }




    public class FFF
    {
        public struct TEST : IAttach, IDetach, ICopyable<TEST>, IDefault
        {
            public void CopyFrom(in TEST other)
            {
                throw new NotImplementedException();
            }

            public void OnAttach(State state)
            {
                throw new NotImplementedException();
            }

            public void OnDetach(State state)
            {
                throw new NotImplementedException();
            }

            public void OnRecycle()
            {
                throw new NotImplementedException();
            }

            void IDefault.Setup()
            {
                throw new NotImplementedException();
            }
        }


        public void GG()
        {
            UnmanagedLayout<ushort, TEST, ushort, TickOffsetData<TEST>> l = default;


            Caller<
                ushort, TEST, ushort, TickOffsetData<TEST>, TEST,
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
                >
                c = default;

            



        }

    }
}
