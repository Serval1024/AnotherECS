using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Actions;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Gen.Common")]
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

        TUintNextNumber,
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
        TSerialize,
        TManualHistory
        >
        : ICaller<TDense>, IDisposable, ISerialize, IRevertCaller, ITickFinishedCaller, IRevertFinishedCaller, IAttachCaller, IDetachCaller, IResizableCaller, IInjectCaller

        where TSparse : unmanaged
        where TDense : unmanaged, IComponent
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged

        where TUintNextNumber : struct, INumberProvier<TDenseIndex>
        where TInject : struct, IInject<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TIdAllocator : struct, IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TDefaultSetter : struct, IDefaultSetter<TDense>
        
        where TAttachDetachStorage : struct, IData, IAttachDetachProvider<TSparse>, IBoolConst
        where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparseStorage : struct, ISparseProvider<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, IIterator<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst, ISingleDenseFlag
        where TDenseStorage : struct, IStartIndexProvider, IDenseProvider<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TIsBindToEntity : struct, IBoolConst
        where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        where TVersion : struct, IChange<TSparse, TDense, TDenseIndex, TTickData>, IVersion<TSparse, TDense, TDenseIndex, TTickData>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>, IRevertFinished, IBoolConst
        where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>, IIterator<TSparse, TDense, TDenseIndex, TTickData>
        where TSerialize : struct, ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TManualHistory : struct, ISegmentHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
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

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _layout != null && _depencies != null;
        }

        public bool IsSingle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TSparseStorage).IsSingleDense;
        }

        public bool IsResizable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => allocator.IsSparseResize<TSparseStorage>();
        }

        public bool IsRevert
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(THistory).IsRevert;
        }

        public bool IsTickFinished
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(THistory).IsTickFinished;
        }

        public bool IsRevertFinished
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TVersion).IsRevertFinished;
        }

        public bool IsSerialize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TSerialize).Is;
        }

        public bool IsAttach
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TAttach).Is;
        }

        public bool IsDetach
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TDetach).Is;
        }

        public bool IsInject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TInject).Is;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* GetLayout()
            => _layout;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GlobalDepencies* GetDepencies()
            => _depencies;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
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
            => default(TIdAllocator).GetCount(ref *_layout, default(TDenseStorage).GetIndex());
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => default(TDenseStorage).GetCapacity(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated()
            => default(TDenseStorage).GetAllocated(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint capacity)
        {
            allocator.SparseResize<TSparseStorage>(ref *_layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach()
        {
            default(TAttach).Attach<TSparseStorage>(_layout, attachDetachStorage.GetState(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach()
        {
            default(TDetach).Detach<TSparseStorage>(_layout, attachDetachStorage.GetState(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallConstruct()
        {
            TInject inject = default;
            if (inject.Is)
            {
                TDenseStorage dense = default;
                default(TSparseStorage).ForEach<ConstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
                default(THistory).ForEach<ConstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallDeconstruct()
        {
            TInject inject = default;
            if (inject.Is)
            {
                TDenseStorage dense = default;
                default(TSparseStorage).ForEach<DeconstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
                default(THistory).ForEach<DeconstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref *_layout, ref *_depencies, dense.GetIndex(), GetCount());
            }
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
            => default(TSparseStorage).IsHas(ref *_layout, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TDense Read(EntityId id)
            => ref UnsafeDirectRead(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense UnsafeDirectRead(EntityId id)
            => ref default(TDenseStorage).GetDense(ref *_layout, default(TSparseStorage).ConvertToDenseIndex(ref *_layout, id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDense* UnsafeDirectReadPtr(EntityId id)
            => default(TDenseStorage).GetDensePtr(ref *_layout, default(TSparseStorage).ConvertToDenseIndex(ref *_layout, id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense Get(EntityId id)
        {
            var denseIndex = default(TSparseStorage).ConvertToDenseIndex(ref *_layout, id);

            ref var component = ref default(TDenseStorage).GetDense(ref *_layout, denseIndex);

            DirectDenseUpdateVersion(denseIndex);

            default(THistory).PushDense<TCopyable, TUintNextNumber>(ref *_layout, ref *_depencies, denseIndex, ref component);

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
            Get(id) = data;
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
        public void Add(EntityId id, ref TDense data)
        {
            ref var component = ref AddInternal(id);
            component = data;

            AddPostInternal(id, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid(EntityId id)
        {
            AddPostInternal(id, ref AddInternal(id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateForId(EntityId id)
        {
            var denseIndex = AllocateForId();

            if (default(TIsBindToEntity).Is)
            {
                _depencies->entities.Add(id, _elementId);
            }

            TSparseStorage sparseStorage = default;
            if (sparseStorage.IsUseSparse)
            {
                sparseStorage.SetSparse<THistory>(ref *_layout, ref *_depencies, id, denseIndex);
            }
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateForId()
        {
            TryResizeDenseInternal();
            return UnsafeAllocateForId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex UnsafeAllocateForId()
        {
            var denseIndex = default(TIdAllocator).AllocateId<THistory, TUintNextNumber>(ref *_layout, ref *_depencies);
            DirectDenseUpdateVersion(denseIndex);
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id)
        {
            TSparseStorage sparseStorage = default;
            THistory history = default;

            var denseIndex = sparseStorage.ConvertToDenseIndex(ref *_layout, id);
            if (sparseStorage.IsUseSparse)
            {
                ref var sparse = ref sparseStorage.GetSparse(ref *_layout, id);
                history.PushSparse(ref *_layout, ref *_depencies, id, sparse);
                sparse = default;
            }
            ref var component = ref default(TDenseStorage).GetDense(ref *_layout, denseIndex);

            history.PushDense<TCopyable, TUintNextNumber>(ref *_layout, ref *_depencies, denseIndex, ref component);

            default(TCopyable).Recycle(ref component);

            default(TIdAllocator).DeallocateId<THistory>(ref *_layout, ref *_depencies, denseIndex);

            default(TDetach).Detach(attachDetachStorage.GetState(), ref component);

            default(TInject).Deconstruct(ref *_layout, ref *_depencies, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(EntityId id)
            => default(TVersion).GetVersion(ref *_layout, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DirectDenseUpdateVersion(TDenseIndex denseIndex)
        {
            default(TVersion).Change(ref * _layout, ref * _depencies, denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DirectPush(TTickDataDense* data)
        {
            default(TManualHistory).PushSegmentDense(ref *_layout, ref *_depencies, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeedResizeDense()
        {
            TDenseStorage denseStorage = default;
            return denseStorage.GetAllocated(ref *_layout) == denseStorage.GetCapacity(ref *_layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
        {
            TDenseStorage denseStorage = default;
            uint capacity = denseStorage.GetCapacity(ref *_layout);
            if (denseStorage.GetAllocated(ref *_layout) == capacity)
            {
                allocator.DenseResize(ref *_layout, capacity << 1);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryIncResizeDense()
        {
            TDenseStorage denseStorage = default;
            uint capacity = denseStorage.GetCapacity(ref *_layout);
            if (denseStorage.GetAllocated(ref *_layout) == capacity)
            {
                allocator.DenseResize(ref *_layout, capacity + 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetStorage()
        {
            Detach();

            TCopyable copyable = default;
            if (copyable.Is)
            {
                copyable.Recycle<TSparse, TDenseIndex, TTickData, TTickDataDense, TSparseStorage>(ref *_layout, ref *_depencies, default(TDenseStorage).GetIndex(), GetCount());
            }
            CallDeconstruct();

            LayoutActions.StorageClear(ref *_layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetHistory()
        {
            default(THistory).HistoryClear<TCopyable>(ref *_layout);
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
            default(THistory).TickFinished<TCopyable>(ref *_layout, ref *_depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
            TDenseStorage dense = default;
            default(TVersion).DropChange(ref *_layout, ref *_depencies, dense.GetIndex(), dense.GetAllocated(ref *_layout));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            default(THistory).RevertTo<TAttachDetachStorage, TAttach, TDetach, TSparseStorage, TVersion, TSparseStorage>
                (_layout, ref attachDetachStorage, tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            default(TSerialize).Pack(ref writer, _layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            default(TSerialize).Unpack(ref reader, _layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref TDense AddInternal(EntityId id) 
            => ref default(TDenseStorage).GetDense(ref *_layout, AllocateForId(id));
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPostInternal(EntityId id, ref TDense component)
        {
            default(TInject).Construct(ref *_layout, ref *_depencies, ref component);
            default(TAttach).Attach(attachDetachStorage.GetState(), ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResizeDenseInternal()
        {
            TDenseStorage denseStorage = default;
            uint capacity = denseStorage.GetCapacity(ref *_layout);
            if (denseStorage.GetAllocated(ref *_layout) == capacity)
            {
                allocator.DenseResize(ref *_layout, capacity << 1);
            }
        }
    }
}
