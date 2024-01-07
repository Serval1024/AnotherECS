using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;
using AnotherECS.Core.Threading;
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
        TAllocator,
        TSparse,
        TDense,
        TDenseIndex,

        TMemoryAllocatorProvider,
        TUintNextNumber,
        TInject,
        TIdAllocator,
        TDefaultSetter,
        TAttachDetachStorage,
        TAttach,
        TDetach,
        TSparseStorage,
        TDenseStorage,
        TBinderToFilters,
        TVersion,
        TSerialize,
        TRebindMemory
        >
        : ICaller<TDense>, ITickFinishedCaller, IRevertFinishedCaller, IRevertStages, IAttachCaller, IDetachCaller, IResizableCaller, IInjectCaller

        where TSparse : unmanaged
        where TDense : unmanaged, IComponent
        where TDenseIndex : unmanaged
        where TAllocator : unmanaged, IAllocator

        where TMemoryAllocatorProvider : struct, IAllocaterProvider<TAllocator, TAllocator>
        where TUintNextNumber : struct, INumberProvier<TDenseIndex>
        where TInject : struct, IInject<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TIdAllocator : struct, IIdAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TDefaultSetter : struct, IData, IDefaultSetter<TDense>
        
        where TAttachDetachStorage : struct, IData, IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAttach : struct, IAttach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TDetach : struct, IDetach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TSparseStorage : struct, ISparseProvider<TAllocator, TSparse, TDense, TDenseIndex>, IIterator<TAllocator, TSparse, TDense, TDenseIndex>, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst, ISingleDenseFlag, IExternalFromCallerConfig
        where TDenseStorage : struct, IStartIndexProvider, IDenseProvider<TAllocator, TSparse, TDense, TDenseIndex>, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TBinderToFilters : struct, IBinderToFilters
        where TVersion : struct, IChange<TAllocator, TSparse, TDense, TDenseIndex>, IVersion<TAllocator, TSparse, TDense, TDenseIndex>, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>, IRevertFinished, IBoolConst
        where TSerialize : struct, ICustomSerialize<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TRebindMemory : struct, IRebindMemory<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
    {
        private UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* _layout;
        private CallerDirtyHandler _dirtyHandler;
        private GlobalDependencies* _dependencies;

        private ushort _elementId;

        private TAttachDetachStorage _attachDetachStorage;
        private TSparseStorage _sparseStorage;
        private TDefaultSetter _defaultSetter;


        private readonly HubLayoutAllocator<
                   TAllocator, TSparse, TDense, TDenseIndex,
                   TSparseStorage, TDenseStorage, TIdAllocator, TVersion, TAttachDetachStorage
                   > layoutAllocator;

        public ushort ElementId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _elementId;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _layout != null && _dependencies != null;
        }

        public bool IsSingle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sparseStorage.IsSingleDense;
        }

        public bool IsResizable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => layoutAllocator.IsSparseResize<TSparseStorage>();
        }

        public bool IsTickFinished
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TBinderToFilters).IsTemporary;
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

        public bool IsTemporary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TBinderToFilters).IsTemporary;
        }

        public bool IsInject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TInject).Is;
        }

        public uint GetDenseMemoryAllocated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _layout->storage.dense.IsValid
                ? (_layout->storage.dense.Length * (uint)sizeof(TDense))
                : 0u;
        }

        public bool IsCallRevertStages
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default(TAttachDetachStorage).Is;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* GetLayout()
            => _layout;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GlobalDependencies* GetDependencies()
            => _dependencies;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => typeof(TDense);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(void* layout, GlobalDependencies* dependencies, ushort id, CallerDirtyHandler dirtyHandler, State state)
        {
            _layout = (UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>*)layout;
            _dependencies = dependencies;
            _elementId = id;
            _dirtyHandler = dirtyHandler;
            _attachDetachStorage.Allocate(state, dependencies);
            _defaultSetter.Allocate(state, dependencies);
            _sparseStorage.Config(dependencies, id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            layoutAllocator.LayoutAllocate(
                ref *_layout,
                ref *_dependencies,
                default(TMemoryAllocatorProvider).Get(_dependencies),
                default(TMemoryAllocatorProvider).Get(_dependencies),
                default(TMemoryAllocatorProvider).Get(_dependencies),
                default(TMemoryAllocatorProvider).Get(_dependencies),
                default(TMemoryAllocatorProvider).GetAlt(_dependencies)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDense Create()
        {
            TDense component = default;
            CreateInternal(ref component);
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _attachDetachStorage.Dispose();
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
            layoutAllocator.SparseResize<TSparseStorage>(ref *_layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach()
        {
            default(TAttach).Attach<TSparseStorage>(ref *_layout, _attachDetachStorage.GetState(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach()
        {
            default(TDetach).Detach<TSparseStorage>(ref *_layout, _attachDetachStorage.GetState(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallConstruct()
        {
            if (default(TInject).Is)
            {
                _sparseStorage.ForEach<ConstructInjectIterable<TAllocator, TSparse, TDense, TDenseIndex>>(ref *_layout, ref *_dependencies, default(TDenseStorage).GetIndex(), GetCount());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallDeconstruct()
        {
            if (default(TInject).Is)
            {
                _sparseStorage.ForEach<DeconstructInjectIterable<TAllocator, TSparse, TDense, TDenseIndex>>(ref *_layout, ref *_dependencies, default(TDenseStorage).GetIndex(), GetCount());
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
            => _sparseStorage.IsHas(ref *_layout, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TDense Read(EntityId id)
            => ref UnsafeDirectRead(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense UnsafeDirectRead(EntityId id)
            => ref default(TDenseStorage).GetDense(ref *_layout, _sparseStorage.ConvertToDenseIndex(ref *_layout, id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDense* UnsafeDirectReadPtr(EntityId id)
            => default(TDenseStorage).GetDensePtr(ref *_layout, _sparseStorage.ConvertToDenseIndex(ref *_layout, id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense Get(EntityId id)
        {
            var denseIndex = _sparseStorage.ConvertToDenseIndex(ref *_layout, id);

            ref var component = ref default(TDenseStorage).GetDense(ref *_layout, denseIndex);

            UpdateVersion(denseIndex);

            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, TDense data)
        {
            Set(id, ref data);
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
            component = Create();
            AddExistPostInternal(ref component);
            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ref TDense data)
        {
            ref var component = ref AddInternal(id);
            component = data;
            AddPostInternal(ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, TDense data)
        {
            Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid(EntityId id)
        {
            AddPostInternal(ref AddInternal(id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id)
        {
            default(TBinderToFilters).Remove(ref *_dependencies, id, _elementId);
            RemoveRaw(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRaw(EntityId id)
        {
            _dirtyHandler.Dirty();

            var denseIndex = _sparseStorage.ConvertToDenseIndex(ref *_layout, id);
            if (_sparseStorage.IsUseSparse)
            {
                ref var sparse = ref _sparseStorage.GetSparse(ref *_layout, id);
                sparse = default;
            }
            ref var component = ref default(TDenseStorage).GetDense(ref *_layout, denseIndex);

            default(TIdAllocator).DeallocateId(ref *_layout, ref *_dependencies, denseIndex);

            default(TDetach).Detach(_attachDetachStorage.GetState(), ref component);

            default(TInject).Deconstruct(ref *_layout, ref *_dependencies, ref component);

            UpdateAddRemove(denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(EntityId id)
            => default(TVersion).GetVersion(ref *_layout, id);

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
                layoutAllocator.DenseResize(ref *_layout, capacity << 1);
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
                layoutAllocator.DenseResize(ref *_layout, capacity + 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetStorage()
        {
            ResetTepmoray();
            _layout->Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            ResetStorage();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetTepmoray()
        {
            CallDeconstruct();

            _layout->storage.denseIndex = default(TDenseStorage).GetIndex();
            _layout->storage.recycleIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            if (default(TBinderToFilters).IsTemporary)
            {
                ResetTepmoray();
                LayoutActions.SparseClear(ref *_layout);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage1()
        {
            _attachDetachStorage.RevertStage1(ref *_layout, GetAllocated());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage2()
        {
            default(TDetach).Detach(ref *_layout, ref _sparseStorage, _attachDetachStorage.GetState(), _attachDetachStorage.GetAddRemoveVersion(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage3()
        {
            default(TAttach).Attach(ref *_layout, ref _sparseStorage, _attachDetachStorage.GetState(), _attachDetachStorage.GetAddRemoveVersion(), default(TDenseStorage).GetIndex(), GetCount());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
            TDenseStorage dense = default;
            default(TVersion).DropChange(ref *_layout, ref *_dependencies, dense.GetIndex(), dense.GetAllocated(ref *_layout));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>()
            where T : unmanaged
            => _sparseStorage.ReadSparse<T>(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> GetDense<T>()
            where T : unmanaged, IComponent
            => default(TDenseStorage).GetDense<T>(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion()
            => default(TVersion).ReadVersion(ref *_layout);

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
            => ref default(TDenseStorage).GetDense(ref *_layout, Allocate(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateInternal(ref TDense component)
        {
            _defaultSetter.SetupDefault(ref component);
            default(TInject).Construct(ref *_layout, ref *_dependencies, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPostInternal(ref TDense component)
        {
            CreateInternal(ref component);
            AddExistPostInternal(ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddExistPostInternal(ref TDense component)
        {
            default(TAttach).Attach(_attachDetachStorage.GetState(), ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryResizeDenseInternal()
        {
            TDenseStorage denseStorage = default;
            uint capacity = denseStorage.GetCapacity(ref *_layout);
            if (denseStorage.GetAllocated(ref *_layout) == capacity)
            {
                _dirtyHandler.Dirty();
                layoutAllocator.DenseResize(ref *_layout, capacity << 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TDenseIndex Allocate(EntityId id)
        {
            var denseIndex = AllocateNext();

            default(TBinderToFilters).Add(ref *_dependencies, id, _elementId);
            if (_sparseStorage.IsUseSparse)
            {
                _sparseStorage.SetSparse(ref *_layout, ref *_dependencies, id, denseIndex);
            }
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TDenseIndex AllocateNext()
        {
            TryResizeDenseInternal();
            return UnsafeAllocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TDenseIndex UnsafeAllocate()
        {
            _dirtyHandler.Dirty();
            var denseIndex = default(TIdAllocator).AllocateId<TUintNextNumber>(ref *_layout, ref *_dependencies);
            UpdateVersion(denseIndex);
            UpdateAddRemove(denseIndex);
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateVersion(TDenseIndex denseIndex)
        {
            default(TVersion).Change(ref *_layout, ref *_dependencies, denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateAddRemove(TDenseIndex denseIndex)
        {
            if (_attachDetachStorage.Is)
            {
                _attachDetachStorage.AddRemoveEvent(ref *_layout, default(TUintNextNumber).ToNumber(denseIndex));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref *_layout, ref rebinder);

            if (default(TRebindMemory).Is)
            {
                _sparseStorage.ForEach<RebindMemoryIterable<TAllocator, TSparse, TDense, TDenseIndex>>(ref *_layout, ref *_dependencies, default(TDenseStorage).GetIndex(), GetCount());
            }
        }
    }
}
