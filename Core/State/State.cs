﻿using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AnotherECS.Serializer;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public abstract unsafe class State : BaseState, ISerialize
    {
        #region const
        private const uint CORE_LAYOUT_COUNT = 0;
        private const int COMPONENT_ENTITY_MAX = 32;
        private const uint FILTER_INIT_CAPACITY = 32;
        #endregion
        #region data
        private RawAllocator _allocator;
        private GlobalDepencies* _depencies;
        private NContainerArray<HAllocator, UnmanagedLayout> _layouts;
        private uint _layoutCount;

        private ICaller[] _callers;

        private Events _events;
        #endregion

        #region data cache
        private ITickFinishedCaller[] _tickFinishedCallers;  //TODO SER MTHREAD
        private ResizableData[] _resizableCallers;  //TODO SER MTHREAD
        #endregion

        #region construct & destruct
        public State()
            : this(StateConfig.Create()) { }

        public State(in StateConfig config)
        {
            SystemInit(config, new TickProvider());

            _layoutCount = 1;
            _layouts = new NContainerArray<HAllocator, UnmanagedLayout>(&_depencies->hAllocator, GetLayoutCount());
            _callers = new ICaller[GetLayoutCount()];

            _events = new Events(config.history.recordTickLength);

            CommonInit();

            _depencies->archetype = new Archetype(_depencies, GetTemporaryIndexes());

            AllocateLayouts();
        }

        internal State(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
        }

        private GlobalDepencies* CreateGlobalDepencies()
            => (GlobalDepencies*)_allocator.Allocate((uint)sizeof(GlobalDepencies)).pointer;

        private void SystemInit(in StateConfig config, in TickProvider tickProvider)
        {
            _allocator = new RawAllocator();
            var basicAllocator = BAllocator.Create();
            _depencies = CreateGlobalDepencies();

            _depencies->config = config;
            _depencies->tickProvider = tickProvider;
            _depencies->bAllocator = basicAllocator;
            _depencies->hAllocator = new HAllocator(
                &_depencies->bAllocator,
                2,
                _depencies->config.general.chunkLimit,
                _depencies->config.history.buffersCapacity,
                _depencies->config.history.recordTickLength);

            _depencies->componentTypesCount = GetComponentCount();
            _depencies->entities = new Entities(_depencies);
            _depencies->injectContainer = new InjectContainer(new NPtr<HAllocator>(&_depencies->hAllocator));
            _depencies->filters = new Filters(_depencies, 32);
        }

        protected override void OnDispose()
        {
            _depencies->bAllocator.Dispose();    //Free all memory.
            _allocator.Deallocate(_depencies);
        }
        #endregion

        #region serialization
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.AddDepency(new NPtr<GlobalDepencies>(_depencies));
            writer.AddDepency(new NPtr<BAllocator>(&_depencies->bAllocator));
            writer.AddDepency(new NPtr<HAllocator>(&_depencies->hAllocator));

            _depencies->Pack(ref writer);
            _events.Pack(ref writer);

            _layouts.Pack(ref writer);

            for(uint i = 1; i < _callers.Length; ++i)
            {
                _callers[i].Pack(ref writer);
            }
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _depencies = CreateGlobalDepencies();

            reader.AddDepency(new NPtr<GlobalDepencies>(_depencies));
            reader.AddDepency(new NPtr<BAllocator>(&_depencies->bAllocator));
            reader.AddDepency(new NPtr<HAllocator>(&_depencies->hAllocator));

            _depencies->Unpack(ref reader);
            _depencies->filters = new Filters(_depencies, FILTER_INIT_CAPACITY);

            _events.Unpack(ref reader);

            _layoutCount = 1;
            _layouts.Unpack(ref reader);

            _callers = new ICaller[GetLayoutCount()];
            CommonInit();

            for (uint i = 1; i < _callers.Length; ++i)
            {
                _callers[i].Unpack(ref reader);
            }
        }

        private void CommonInit()
        {
            BindingCodeGenerationStage(_depencies->config);

            _tickFinishedCallers = _callers.Skip(1).Where(p => p.IsTickFinished && p is ITickFinishedCaller).Cast<ITickFinishedCaller>().ToArray();
            _resizableCallers = _callers
                .Skip(1)
                .Where(p => p.IsResizable && p is IResizableCaller)
                .Cast<IResizableCaller>()
                .Select((p, i) => new ResizableData() { caller = p, callerIndex = (uint)i + 1u })
                .ToArray();
        }
        #endregion

        #region init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FirstStartup() //TODO SER inter
        {
            for (int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i].IsAttach && _callers[i] is IAttachCaller callerAttach)
                {
                    callerAttach.Attach();
                }
            }
            for (int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i].IsInject && _callers[i] is IInjectCaller injectCaller)
                {
                    injectCaller.CallConstruct();
                }
            }
        }
        #endregion

        #region entity
        public uint EntityCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _depencies->entities.GetCount();
            }
        }

        public bool IsHas(uint id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _depencies->entities.IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId New()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_depencies->entities.TryResizeDense())
            {
                ResizeStorages(_depencies->entities.GetCapacity());
            }
            var id = _depencies->entities.Allocate();
            _depencies->archetype.Add(id);
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeStorages(uint capacity)
        {
            for(int i = 0; i < _resizableCallers.Length; ++i)
            {
                _layouts.Dirty(_resizableCallers[i].callerIndex);
                _resizableCallers[i].caller.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity NewEntity()
            => EntityExtensions.Pack(this, New());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id);
            if (_depencies->archetype.GetCount(_depencies->entities.ReadArchetypeId(id)) > COMPONENT_ENTITY_MAX)
            {
                throw new Exceptions.ReachedLimitComponentOnEntityException(COMPONENT_ENTITY_MAX);
            }
#endif
            var componentIds = stackalloc uint[COMPONENT_ENTITY_MAX];
            var archetypeId = _depencies->entities.ReadArchetypeId(id);
            var count = _depencies->archetype.GetItemIds(archetypeId, componentIds, COMPONENT_ENTITY_MAX);

            for(int i = 0; i < count; ++i)
            {
                GetCaller(componentIds[i]).RemoveRaw(id);
            }

            _depencies->archetype.Remove(archetypeId, id);
            _depencies->entities.Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _depencies->archetype.GetCount(_depencies->entities.ReadArchetypeId(id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHas(EntityId id, ushort generation)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _depencies->entities.IsHas(id, generation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetGeneration(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _depencies->entities.ReadGeneration(id);
        }
        #endregion

        #region multi component
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Create<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return GetCaller<T>().Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>(EntityId id)
          where T : unmanaged, IComponent
        {

#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNotMultiAccess(this, id, GetCaller<T>());
#endif
            return GetCaller<T>().IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, T data)
            where T : unmanaged, IComponent
            => Add(id, ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            GetCaller<T>().Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T>().Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            Add<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            GetCaller<T>().Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(EntityId id, uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller(_depencies->archetype.GetItemId(_depencies->entities.ReadArchetypeId(id), index)));
#endif
            return GetCaller(_depencies->archetype.GetItemId(_depencies->entities.ReadArchetypeId(id), index)).GetCopy(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T>().Read(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T>().Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, uint index, IComponent component)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller(_depencies->archetype.GetItemId(_depencies->entities.ReadArchetypeId(id), index)));
#endif
            GetCaller(_depencies->archetype.GetItemId(_depencies->entities.ReadArchetypeId(id), index)).Set(id, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            GetCaller<T>().Set(id, ref data);
        }
        #endregion

        #region single component
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            return GetCaller<T>().IsHas(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(T data)
            where T : unmanaged, ISingle
            => SetOrAdd(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            GetCaller<T>().SetOrAdd(0, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : unmanaged, ISingle
            => Add(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Add(0, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Add(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Remove(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Read(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Get<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Get(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Set(0, ref data);
        }
        #endregion

        #region other public api
        public uint Tick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _depencies->tickProvider.tick;
            }
            private set
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                _depencies->tickProvider.tick = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(BaseEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            Send(new EventContainer(_depencies->tickProvider.tick + 1, @event));
        }
        #endregion

        #region filters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FilterBuilder CreateFilterBuilder()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filter<T0> Filter<T0>()
            where T0 : IComponent
            => CreateFilterBuilder().With<T0>().Build();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filter<T0, T1> Filter<T0, T1>()
            where T0 : IComponent
            where T1 : IComponent
            => CreateFilterBuilder().With<T0>().With<T1>().Build();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lock()
        {
            _depencies->filters.Lock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Unlock()
        {
            _depencies->filters.Unlock();
        }
        #endregion

        #region events
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStarted() //TODO SER internal
        {
            ++Tick;
            _events.TickStarted(Tick);
            _depencies->hAllocator.TickStarted(Tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished() //TODO SER internal
        {
            foreach (var tickFinished in _tickFinishedCallers)
            {
                tickFinished.TickFinished();
            }
        }
        #endregion

        #region internal api
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T CreateFilter<T>(ref Mask mask)
            where T : BFilter, new()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (!mask.IsValide)
            {
                throw new Exceptions.MaskIsEmptyException();
            }

            var filter = new T();
            filter.Construct(this, CreateFilterData(ref mask));
            return filter;
        }

        internal FilterData* CreateFilterData(ref Mask mask)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _depencies->filters.Create(ref mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref readonly IdCollection<HAllocator> GetEntitiesByArchetype(uint archetypeId)
        { 
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return ref _depencies->archetype.GetIdCollection(archetypeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetIdByType<T>()
            where T : IComponent
            => GetIndex<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(ITickEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            _events.Send(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(List<ITickEvent> result)
            => GetEvent(Tick, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(uint tick, List<ITickEvent> result)
            => _events.Find(tick, result);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetNextTickForEvent()
            => _events.NextTickForEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RebindMemoryHandles()
        {
            var memoryRebinder = MemoryRebinderUtils.Create(&_depencies->bAllocator, &_depencies->hAllocator);
            _depencies->currentMemoryRebinder = memoryRebinder;

            for (uint i = 1; i < _layoutCount; ++i)
            {
                _callers[i].RebindMemoryHandle(ref memoryRebinder);
            }

            MemoryRebinderCaller.Rebind(ref _depencies->entities, ref memoryRebinder);
            MemoryRebinderCaller.Rebind(ref _depencies->archetype, ref memoryRebinder);

            _depencies->currentMemoryRebinder = default;
            memoryRebinder.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RevertTo(uint tick)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_depencies->hAllocator.RevertTo(tick))
            {
                RebindMemoryHandles();
            }

            Tick = tick;
        }

        private NArray<BAllocator, ushort> GetTemporaryIndexes()
        {
            using var list = new NList<BAllocator, ushort>(&_depencies->bAllocator, FILTER_INIT_CAPACITY);

            for(int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i].IsTemporary)
                {
                    list.Add(_callers[i].ElementId);
                }
            }
            return list.ToNArray();
        }
        #endregion

        #region helpers
        private uint GetLayoutCount()
            => GetComponentCount() + 1 + CORE_LAYOUT_COUNT;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller<T> GetCaller<T>(ushort index)
            where T : unmanaged, IComponent
#if !ANOTHERECS_RELEASE
            => (ICaller<T>)_callers[index];
#else
            => Unsafe.UnsafeUtils.As<ICaller, ICaller<T>>(ref _callers[index]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller<T> GetCaller<T>()
            where T : unmanaged, IComponent
            => GetCaller<T>(GetIndex<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller GetCaller(uint index)
            => _callers[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UCaller AddLayout<UCaller, TAllocator, TSparse, TDense, TDenseIndex>(ComponentFunction<TDense> componentFunction = default)
            where TAllocator : unmanaged, IAllocator
            where UCaller : struct, ICallerReference
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (_layoutCount == GetLayoutCount())
            {
                throw new System.InvalidOperationException();
            }
#endif
            var layout = (UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>*)_layouts.GetPtr(_layoutCount);
            layout->componentFunction = componentFunction;

            return AddLayout<UCaller>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UCaller AddLayout<UCaller>(UCaller caller = default)
            where UCaller : struct, ICallerReference
        {
#if !ANOTHERECS_RELEASE
            if (_layoutCount == GetLayoutCount())
            {
                throw new System.InvalidOperationException();
            }
#endif
            var icaller = (ICaller)caller;
            _callers[_layoutCount] = icaller;

            icaller.Config(
                _layouts.GetPtr(_layoutCount),
                _depencies,
                (ushort)_layoutCount,
                _layouts.GetDirtyHandler(_layoutCount),
                this);

            ++_layoutCount;

            return (UCaller)icaller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EFastAccess CreateFA<TComponent, EFastAccess>()
            where TComponent : IComponent
            where EFastAccess : struct, IFastAccess
        {
            EFastAccess fastAccess = default;
            fastAccess.Config(_callers[GetIndex<TComponent>()]);
            return fastAccess;
        }

        private void AllocateLayouts()
        {
            for (int i = 1; i < _layoutCount; ++i)
            {
                _callers[i].AllocateLayout();
            }
        }
        #endregion

        #region codegen & abstract
        protected abstract void BindingCodeGenerationStage(in StateConfig config);
        protected abstract uint GetComponentCount();
        protected abstract ushort GetIndex<T>()
            where T : IComponent;
        #endregion

        #region declarations
        private struct ResizableData
        {
            public uint callerIndex;
            public IResizableCaller caller;
        }
        #endregion
    }
}

