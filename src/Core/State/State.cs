using AnotherECS.Core.Allocators;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Jobs")]
namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public abstract unsafe class State : BDisposable, IState, ISerializeConstructor
    {
        #region const
        private const int CALLER_START_INDEX = 1;
        private const uint FILTER_INIT_CAPACITY = 32;

        private const uint BASIC_ALLOCATOR_ID = 1;
        private const uint HISTORY_ALLOCATOR_STAGE0_ID = 2;
        private const uint HISTORY_ALLOCATOR_STAGE1_ID = 3;
        #endregion

        #region internal
        internal bool IsCalledStartup => _isFirstStartup;
        #endregion

        #region data
        private RawAllocator _allocator;
        private Dependencies* _dependencies;        
        private uint _layoutCount;
        private uint _nextTickForEvent;

        private IModuleData[] _moduleDatas;

        private ICaller[] _callers;
        private Dictionary<Type, ICaller> _callerByType;

        private IConfig[] _configs;
        private Dictionary<Type, uint> _configByType;
        
        private StateOption _option;
        private Events _events;
        private Signals _signals;

        private ushort _stateId;
        private NeedRefreshByTick _reference;

        #endregion

        #region startup data
        private bool _isFirstStartup;
        #endregion

        #region threading
        private readonly object _eventLock = new();
        #endregion

        #region data cache
        private IRevertStages[] _revertStagesCallers;
        private IRepairStateId[] _repairStateIdCallers;
        private ResizableData[] _resizableCallers;
        private List<ITickEvent> _eventsTemp;
        private List<SignalCallback> _signalsTemp;
        private EntityId[] _entityIdsTemp;
        #endregion

        #region Error
        private Exception _lastError;
        #endregion

        #region construct & destruct
        public State()
            : this(StateConfig.Create()) { }

        public State(in StateConfig config)
        {
            ConstructInternal(config);
        }

        public State(ref ReaderContextSerializer reader)
        {
            _stateId = StateGlobalRegister.Register(this);

            Unpack(ref reader);
        }

        internal void SetOption(StateOption option)
        {
            _option = option;
        }

        private void ConstructInternal(in StateConfig config)
        {
            _stateId = StateGlobalRegister.Register(this);

            SystemInit(config);

            _layoutCount = 1;
            _callers = new ICaller[GetComponentArrayCount()];
            _configs = new IConfig[GetConfigArrayCount()];

            _events = new Events(config.history.recordTickLength);
            _signals = new Signals(config.history.recordTickLength);

            CommonInit();

            _dependencies->archetype = new Archetype(_dependencies, GetTemporaryIndexes());

            AllocateLayouts();
        }

        private Dependencies* CreateDependencies()
            => (Dependencies*)_allocator.Allocate<Dependencies>().pointer;

        private void SystemInit(in StateConfig config)
        {
            _allocator = new RawAllocator();
            _dependencies = CreateDependencies();

            _dependencies->config = config;
            _dependencies->tickProvider = new TickProvider();

            _dependencies->bAllocator = new BAllocator(BASIC_ALLOCATOR_ID);

            _dependencies->stage0HAllocator = new HAllocator(
                &_dependencies->bAllocator,
                HISTORY_ALLOCATOR_STAGE0_ID,
                _dependencies->config.general.chunkLimit,
                _dependencies->config.history.buffersCapacity,
                _dependencies->config.history.recordTickLength);

            _dependencies->stage1HAllocator = new HAllocator(
                &_dependencies->bAllocator,
                HISTORY_ALLOCATOR_STAGE1_ID,
                _dependencies->config.general.chunkLimit,
                _dependencies->config.history.buffersCapacity,
                _dependencies->config.history.recordTickLength);

            _dependencies->componentTypesCount = GetComponentCount();
            _dependencies->entities = new Entities(_dependencies);

            Dependencies.FillCommon(_dependencies, _allocator, FILTER_INIT_CAPACITY);

            _dependencies->stateId = _stateId;
        }

        protected override void OnDispose()
        {
            StateGlobalRegister.Unregister(_stateId);

            foreach (var config in _configs)
            {
                if (config is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            foreach (var data in _moduleDatas)
            {
                if (data is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _dependencies->Dispose();                   //Free all memory.
            _allocator.Deallocate(_dependencies);
        }
        #endregion

        #region serialization
        public void Pack(ref WriterContextSerializer writer)
        {
            var stateSerializationLevel = writer.Dependency.DirectGet<StateSerializationLevel>();

            if (stateSerializationLevel.HasFlag(StateSerializationLevel.Data))
            {
                writer.Dependency.Add(new WPtr<Dependencies>(_dependencies));
                writer.Dependency.Add(_dependencies->bAllocator.GetId(), new WPtr<BAllocator>(&_dependencies->bAllocator));
                writer.Dependency.Add(_dependencies->stage0HAllocator.GetId(), new WPtr<HAllocator>(&_dependencies->stage0HAllocator));
                writer.Dependency.Add(_dependencies->stage1HAllocator.GetId(), new WPtr<HAllocator>(&_dependencies->stage1HAllocator));

                writer.Write(stateSerializationLevel);
                writer.Write(_dependencies->bAllocator.GetId());
                writer.Write(_dependencies->stage0HAllocator.GetId());
                writer.Write(_dependencies->stage1HAllocator.GetId());

                _dependencies->Pack(ref writer);
                _events.Pack(ref writer);
                _signals.Pack(ref writer);

                for (uint i = CALLER_START_INDEX; i < _callers.Length; ++i)
                {
                    _callers[i].Pack(ref writer);
                }
            }
            if (stateSerializationLevel.HasFlag(StateSerializationLevel.Config))
            {
                writer.Pack(_configs);
            }
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            var stateSerializationLevel = reader.ReadEnum<StateSerializationLevel>();

            if (stateSerializationLevel.HasFlag(StateSerializationLevel.Data))
            {
                _dependencies = CreateDependencies();

                var bAllocatorId = reader.ReadUInt32();
                var stage0HAllocatorId = reader.ReadUInt32();
                var stage1HAllocatorId = reader.ReadUInt32();

                reader.Dependency.Add(new WPtr<Dependencies>(_dependencies));
                reader.Dependency.Add(bAllocatorId, new WPtr<BAllocator>(&_dependencies->bAllocator));
                reader.Dependency.Add(stage0HAllocatorId, new WPtr<HAllocator>(&_dependencies->stage0HAllocator));
                reader.Dependency.Add(stage1HAllocatorId, new WPtr<HAllocator>(&_dependencies->stage1HAllocator));

                _dependencies->Unpack(ref reader);
                Dependencies.FillCommon(_dependencies, _allocator, FILTER_INIT_CAPACITY);

                reader.Dependency.Add(RepairMemoryUtils.Create(_dependencies));

                _events.Unpack(ref reader);
                _signals.Unpack(ref reader);

                _layoutCount = 1;

                _callers = new ICaller[GetComponentArrayCount()];
                CommonInit();

                for (uint i = CALLER_START_INDEX; i < _callers.Length; ++i)
                {
                    _callers[i].Unpack(ref reader);
                }
            }
            else
            {
                ConstructInternal(StateConfig.Create());
            }

            if (stateSerializationLevel.HasFlag(StateSerializationLevel.Config))
            {
                _configs = reader.Unpack<IConfig[]>();
            }
            else
            {
                _configs = new IConfig[GetConfigArrayCount()];
            }

            CallConstruct();
            RepairMemoryHandles();

            if (_dependencies->stateId != _stateId)
            {
                _dependencies->stateId = _stateId;
                _reference.Set(Tick);
                TryRepairStateId(Tick);
            }
        }

        private void CommonInit()
        {
            _moduleDatas = Array.Empty<IModuleData>();

            BindingCodeGenerationStage(_dependencies->config);

            StateHelpers.CacheInit(_callers, CALLER_START_INDEX, ref _revertStagesCallers, p => p.IsCallRevertStages);
            StateHelpers.CacheInit(_callers, CALLER_START_INDEX, ref _repairStateIdCallers, p => p.IsRepairStateId);

            StateHelpers.CacheInit<ResizableData, IResizableCaller>(
                _callers,
                CALLER_START_INDEX, 
                ref _resizableCallers, 
                p => p.IsRepairStateId, 
                (p, i) => new ResizableData() { caller = p, callerIndex = (uint)i + 1u });

            _eventsTemp = new();
            _signalsTemp = new();
            _entityIdsTemp = Array.Empty<EntityId>();
            _nextTickForEvent = _events.NextTickForEvent;

            StateHelpers.CacheInit(_callers, CALLER_START_INDEX, ref _callerByType);
            _configByType = new Dictionary<Type, uint>();
        }
        #endregion

        #region init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
        {
            if (!_isFirstStartup)
            {
                _isFirstStartup = true;
                CallAttach();
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
                Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _dependencies->entities.GetCount();
            }
        }

        public bool IsHas(uint id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _dependencies->entities.IsHas(id);
        }

        public EntityId New()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_dependencies->entities.TryResizeDense())
            {
                ResizeStorages(_dependencies->entities.GetCapacity());
            }
            var id = _dependencies->entities.Allocate();
            _dependencies->archetype.Add(id);
            return id;
        }

        public void Delete(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            var archetypeId = _dependencies->entities.ReadArchetypeId(id);
            _dependencies->archetype.ForEachItem(archetypeId, new RemoveRawIterator(this, id));

            _dependencies->archetype.Remove(archetypeId, id);
            _dependencies->entities.Deallocate(id);
        }

        public uint GetCount(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _dependencies->archetype.GetCount(_dependencies->entities.ReadArchetypeId(id));
        }

        internal bool IsHas(EntityId id, ushort generation)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _dependencies->entities.IsHas(id, generation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetGeneration(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _dependencies->entities.ReadGeneration(id);
        }
        #endregion

        #region multi component
        public T Create<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return GetCaller<T>().Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id, uint index)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return index < GetCount(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id, Type type)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfNotMultiAccess(this, id, GetCaller(type));
#endif
            return GetCaller(type).IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>(EntityId id)
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfNotMultiAccess(this, id, GetCaller<T>());
#endif
            return GetCaller<T>().IsHas(id);
        }

        public void Add(EntityId id, IComponent data)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfNotMultiAccess(this, id, GetCaller(data.GetType()));
#endif
            GetCaller(data.GetType()).Add(id, data);
        }

        public void Add<T>(EntityId id, T data)
            where T : unmanaged, IComponent
            => Add(id, ref data);

        public void Add<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            GetCaller<T>().Add(id, ref data);
        }

        public ref T Add<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T>().Add(id);
        }

        public void AddVoid<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            Add<T>(id);
        }

        public void Remove(EntityId id, uint index)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, index, GetCount(id), GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)));
#endif
            GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)).Remove(id);
        }

        public void Remove<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            GetCaller<T>().Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(EntityId id, out T component)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return GetCaller<T>().TryRead(id, out component);
        }

        public IComponent Read(EntityId id, uint index)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, index, GetCount(id), GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)));
#endif
            return GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)).GetCopy(id);
        }

        public ref readonly T Read<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            return ref GetCaller<T>().Read(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(EntityId id, out T component)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return GetCaller<T>().TryGet(id, out component);
        }

        public ref T Get<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            return ref GetCaller<T>().Get(id);
        }

        public uint GetVersion<T>(EntityId id)
          where T : unmanaged, IVersion
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            return GetCaller<T>().GetVersion(id);
        }

        public void Set(EntityId id, uint index, IComponent component)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, index, GetCount(id), GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)));
#endif
            GetCaller(_dependencies->archetype.GetItemId(_dependencies->entities.ReadArchetypeId(id), index)).Set(id, component);
        }

        public void Set<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            GetCaller<T>().Set(id, ref data);
        }

        public EntityId[] CollectAllEntityIds()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_entityIdsTemp.Length != EntityCount)
            {
                _entityIdsTemp = new EntityId[EntityCount];
            }

            int index = 0;
            for(uint i = 1, iMax = _dependencies->entities.GetAllocated(); i < iMax; ++i)
            {
                if (_dependencies->entities.IsHas(i))
                {
                    _entityIdsTemp[index++] = i;
                }
            }

            return _entityIdsTemp;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeStorages(uint capacity)
        {
            for (int i = 0; i < _resizableCallers.Length; ++i)
            {
                _resizableCallers[i].caller.Resize(capacity);
            }
        }
        #endregion

        #region single component
        public bool IsHas<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            return GetCaller<T>().IsHas(0);
        }

        public void SetOrAdd<T>(T data)
            where T : unmanaged, ISingle
            => SetOrAdd(ref data);

        public void SetOrAdd<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            GetCaller<T>().SetOrAdd(0, ref data);
        }

        public void Add<T>(T data)
            where T : unmanaged, ISingle
            => Add(ref data);

        public void Add<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Add(0, ref data);
        }

        public ref T Add<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Add(0);
        }

        public void Remove<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Remove(0);
        }

        public ref readonly T Read<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            return ref GetCaller<T>().Read(0);
        }

        public ref T Get<T>()
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            return ref GetCaller<T>().Get(0);
        }

        public void Set<T>(ref T data)
            where T : unmanaged, ISingle
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
            Exceptions.ExceptionHelper.ThrowIfEmpty(GetCaller<T>());
#endif
            GetCaller<T>().Set(0, ref data);
        }
        #endregion

        #region config
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasConfig<T>()
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            lock (_configs)
            {
                return _configs[GetConfigIndex<T>()] != null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            lock (_configs)
            {
                AddConfigInternal(data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfExists<T>(this, GetConfigIndex<T>(), _configs);
#endif
            lock (_configs)
            {
                AddConfigInternal(data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveConfig<T>()
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfDontExists<T>(this, GetConfigIndex<T>(), _configs);
#endif
            lock (_configs)
            {
                _configs[GetConfigIndex<T>()] = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetConfig<T>()
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfDontExists<T>(this, GetConfigIndex<T>(), _configs);
#endif
            lock (_configs)
            {
                return (T)_configs[GetConfigIndex<T>()];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfDontExists<T>(this, GetConfigIndex<T>(), _configs);
#endif
            lock (_configs)
            {
                _configs[GetConfigIndex<T>()] = data;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasConfig(Type type)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            lock (_configs)
            {
                return _configByType.TryGetValue(type, out var id) && _configs[id] != null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IConfig GetConfig(Type type)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfDontExists(this, type, _configs, _configByType);
#endif
            lock (_configs)
            {
                return _configs[_configByType[type]];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddConfig(IConfig data)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
            Exceptions.ExceptionHelper.ThrowIfExists(this, data.GetType(), _configs, _configByType);
#endif
            lock (_configs)
            {
                var type = data.GetType();
                AddConfigInternal(data, GetConfigIndex(type));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddConfigInternal<T>(T data)
            where T : IConfig
        {
            AddConfigInternal(data, GetConfigIndex<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddConfigInternal(IConfig data, ushort index)
        {
            var type = data.GetType();
            _configs[index] = data;
            if (!_configByType.ContainsKey(type))
            {
                _configByType.Add(type, index);
            }
        }
        #endregion

        #region other public api
        public uint Tick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if !ANOTHERECS_RELEASE
                Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _dependencies->tickProvider.tick;
            }
            private set
            {
#if !ANOTHERECS_RELEASE
                Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
                _dependencies->tickProvider.tick = value;
            }
        }

        public void Send(IEvent @event)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            Send(new EventContainer(_dependencies->tickProvider.tick + 1, @event));
        }

        public void SendSignal<TSignal>(TSignal signal = default)
            where TSignal : ISignal
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            _signals.Send(_dependencies->tickProvider.tick, GetSignalIndex<TSignal>(), signal);
        }
        #endregion

        #region Error
        public Exception GetLastError()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _lastError;
        }
        #endregion

        #region filters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FilterBuilder Filter()
            => new(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QFilter<TComponent> QFilter<TComponent>()
            where TComponent : unmanaged, IComponent
            => new(this, GetCaller<TComponent>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T CreateFilter<T>(ref Mask mask)
            where T : BFilter, new()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            var filter = new T();
            filter.Construct(this, CreateFilterData(ref mask));
            return filter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BFilter CreateFilter(Type type, ref Mask mask)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            var filter = (BFilter)Activator.CreateInstance(type);
            filter.Construct(this, CreateFilterData(ref mask));
            return filter;
        }

        internal FilterData* CreateFilterData(ref Mask mask)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _dependencies->filters.Create(ref mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LockFilter()
        {
            _dependencies->filters.Lock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlockFilter()
        {
            _dependencies->filters.Unlock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQFilter CreateQFilter(Type type, Type mask)
        {
            var filter = (IQFilter)Activator.CreateInstance(type);
            filter.Construct(this, GetCaller(mask));
            return filter;
        }
        #endregion

        #region events
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickStarted()
        {            
            ++Tick;
            _dependencies->stage0HAllocator.TickStarted(Tick);
            _dependencies->stage1HAllocator.TickStarted(Tick);

            FlushEvents();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickFinished()
        {
            _dependencies->stage0HAllocator.TickFinished();
            _dependencies->stage1HAllocator.TickFinished();

            TryDropRepairStateId();

            FlushFireSignals();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickReachedAfterRevert()
        {
            FlushCancelSignals();
        }
        #endregion

        #region internal api
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref readonly IdCollection<HAllocator> GetEntitiesByArchetype(uint archetypeId)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return ref _dependencies->archetype.ReadIdCollection(archetypeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref readonly EntityData GetEntityData(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return ref _dependencies->entities.ReadRef(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHasModuleData(uint id)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return (id < _moduleDatas.Length) && _moduleDatas[id] != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TModuleData GetModuleData<TModuleData>(uint id)
            where TModuleData : IModuleData
        {
            lock (_moduleDatas)
            {
#if !ANOTHERECS_RELEASE
                Exceptions.ExceptionHelper.ThrowIfDisposed(this);
                if (id >= _moduleDatas.Length || _moduleDatas[id] is not TModuleData)
                {
                    throw new ArgumentException(nameof(id));
                }
#endif
                return (TModuleData)_moduleDatas[id];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetModuleData<TModuleData>(uint id, TModuleData data)
            where TModuleData : IModuleData
        {
            lock (_moduleDatas)
            {
#if !ANOTHERECS_RELEASE
                Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
                if (id >= _moduleDatas.Length)
                {
                    Array.Resize(ref _moduleDatas, (int)id + 1);
                }

                _moduleDatas[id] = data;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetIdByType<T>()
            where T : IComponent
            => GetIndex<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetIdByType(Type type)
            => _callerByType[type].ElementId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(ITickEvent @event)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            lock (_eventLock)
            {
                _events.Send(@event);
                _nextTickForEvent = _events.NextTickForEvent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FlushEvents()
        {
            _eventsTemp.Clear();
            CollectEvent(Tick, _eventsTemp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal List<ITickEvent> GetEventCache() 
            => _eventsTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CollectEvent(uint tick, List<ITickEvent> result)
        {
            lock (_eventLock)
            {
                _events.CollectForProcessing(tick, result);
                _nextTickForEvent = _events.NextTickForEvent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetNextTickForEvent()
            => _nextTickForEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetStateId()
            => _stateId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong GetMemoryTotal()
            => _dependencies->bAllocator.BytesAllocatedTotal;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ulong GetHistoryMemoryTotal()
            => _dependencies->stage0HAllocator.HistoryBytesAllocatedTotal + _dependencies->stage1HAllocator.HistoryBytesAllocatedTotal;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RevertTo(uint tick)
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (tick < Tick)
            {
                if ((Tick - tick) > _dependencies->config.history.recordTickLength)
                {
                    _lastError = new Exceptions.HistoryRevertTickLimitException(Tick, tick, _dependencies->config.history.recordTickLength);
                    return;
                }

                _signals.RevertTo(tick);

                if (IsNeedCallRevertByStages())
                {
                    CallRevertStage0();

                    if (_dependencies->stage0HAllocator.RevertTo(tick))
                    {
                        RepairMemoryHandles();
                    }

                    CallRevertStage1();

                    if (_dependencies->stage1HAllocator.RevertTo(tick))
                    {
                        CallConstruct();
                        RepairMemoryHandles();
                        TryRepairStateId(Tick);
                    }

                    CallRevertStage2();
                }
                else
                {
                    if (_dependencies->stage1HAllocator.RevertTo(tick))
                    {
                        CallConstruct();
                        RepairMemoryHandles();
                        TryRepairStateId(tick);
                    }
                }

                Tick = tick;
            }
        }
        #endregion

        #region singals
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FlushSignalCache(List<SignalCallback> buffer)
        {
            buffer.Clear();
            if (_signalsTemp.Count != 0)
            {
                lock (_signalsTemp)
                {
                    for (int i = 0; i < _signalsTemp.Count; ++i)
                    {
                        buffer.Add(_signalsTemp[i]);
                    }
                    _signalsTemp.Clear();
                }
            }
        }
        #endregion

        #region raw api
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<TSparse> ReadSparse<T, TSparse>()
            where T : unmanaged, IComponent
            where TSparse : unmanaged
            => GetCaller<T>().ReadSparse<TSparse>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<T> GetDense<T>()
            where T : unmanaged, IComponent
            => GetCaller<T>().GetDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<T> ReadDense<T>()
            where T : unmanaged, IComponent
            => GetCaller<T>().ReadDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<uint> ReadVersion<T>()
            where T : unmanaged, IComponent
            => GetCaller<T>().ReadVersion();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Dependencies* GetDependencies()
            => _dependencies;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetEntityIdMax()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _dependencies->entities.GetAllocated();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SystemRegisters GetSystemData()
        {
#if !ANOTHERECS_RELEASE
            Exceptions.ExceptionHelper.ThrowIfDisposed(this);
#endif
            return GetSystemRegisters();
        }
        #endregion

        #region private
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushFireSignals()
        {
            var buffer = _signals.GetCurrentTickBuffer();

            if (!buffer.IsEmpty)
            {
                lock (_signalsTemp)
                {
                    for (int i = 0; i < buffer.Length; ++i)
                    {
                        switch(buffer[i].Command)
                        {
                            case Signals.SignalEvent.CommandType.Fire:
                                {
                                    _signalsTemp.Add(new(SignalCallback.CommandType.Fire, buffer[i].Signal));
                                    break;
                                }
                            case Signals.SignalEvent.CommandType.LeaveBuffer:
                                {
                                    _signalsTemp.Add(new(SignalCallback.CommandType.LeaveBuffer, buffer[i].Signal));
                                    break;
                                }
                        }
                    }
                }
            }

            _signals.ClearCurrentTickBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FlushCancelSignals()
        {
            if (_signals.IsDiffBuffer())
            {
                var buffer = _signals.GetDiffBuffer();

                lock (_signalsTemp)
                {
                    for (int i = 0; i < buffer.Length; ++i)
                    {
                        _signalsTemp.Add(new(SignalCallback.CommandType.Cancel, buffer[i].Signal));
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CallAttach()
        {
            for (int i = CALLER_START_INDEX; i < _callers.Length; ++i)
            {
                if (_callers[i].IsAttach && _callers[i] is IAttachCaller callerAttach)
                {
                    callerAttach.Attach();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CallConstruct()
        {
            for (uint i = CALLER_START_INDEX; i < _callers.Length; ++i)
            {
                if (_callers[i].IsInject && _callers[i] is IInjectCaller injectCaller)
                {
                    injectCaller.CallConstruct();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsNeedCallRevertByStages()
            => _revertStagesCallers.Length != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CallRevertStage0()
        {
            for (uint i = 0; i < _revertStagesCallers.Length; ++i)
            {
                _revertStagesCallers[i].RevertStage0();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CallRevertStage1()
        {
            for (uint i = 0; i < _revertStagesCallers.Length; ++i)
            {
                _revertStagesCallers[i].RevertStage1();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CallRevertStage2()
        {
            for (uint i = 0; i < _revertStagesCallers.Length; ++i)
            {
                _revertStagesCallers[i].RevertStage2();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RepairMemoryHandles()
        {
            var repairMemory = RepairMemoryUtils.Create(_dependencies);

            for (uint i = CALLER_START_INDEX; i < _layoutCount; ++i)
            {
                _callers[i].RepairMemoryHandle(ref repairMemory);
            }

            RepairMemoryCaller.Repair(ref _dependencies->entities, ref repairMemory);
            RepairMemoryCaller.Repair(ref _dependencies->archetype, ref repairMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryRepairStateId(uint tick)
        {
            if (_reference.Check(tick))
            {
                _reference.Set(tick);
                for (uint i = 0; i < _repairStateIdCallers.Length; ++i)
                {
                    _repairStateIdCallers[i].RepairStateId(_stateId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryDropRepairStateId()
        {
            _reference.TryDrop(Tick);
        }

        private NArray<BAllocator, uint> GetTemporaryIndexes()
        {
            using var list = new NList<BAllocator, uint>(&_dependencies->bAllocator, FILTER_INIT_CAPACITY);

            for(int i = CALLER_START_INDEX; i < _callers.Length; ++i)
            {
                if (_callers[i].IsTemporary)
                {
                    list.Add(_callers[i].ElementId);
                }
            }
            
            return list.ToNArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetComponentArrayCount()
            => GetComponentCount() + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetConfigArrayCount()
            => GetConfigCount() + 1;

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
        private ICaller GetCaller(Type type)
            => _callerByType[type];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddLayout<UCaller, TDense>()
            where UCaller : struct, ICallerReference
            where TDense : unmanaged
        {
            AddLayout<UCaller, TDense>(default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddLayout<UCaller, TDense>(ComponentFunction<TDense> componentFunction)
            where UCaller : struct, ICallerReference
            where TDense : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (_layoutCount == GetComponentArrayCount())
            {
                throw new InvalidOperationException();
            }
#endif
            AddLayout<UCaller, TDense>(default, componentFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddLayout<UCaller, TDense>(UCaller caller, ComponentFunction<TDense> componentFunction = default)
            where UCaller : struct, ICallerReference
            where TDense : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (_layoutCount == GetComponentArrayCount())
            {
                throw new InvalidOperationException();
            }
#endif
            
            var iCaller = (ICaller<TDense>)caller;

            iCaller.Config(
                _dependencies,
                (ushort)_layoutCount,
                this,
                componentFunction);

            _callers[_layoutCount] = iCaller;

            ++_layoutCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TFastAccess CreateFA<TComponent, TFastAccess>()
            where TComponent : IComponent
            where TFastAccess : struct, IFastAccess
        {
            TFastAccess fastAccess = default;
            fastAccess.Config(_callers[GetIndex<TComponent>()]);
            return fastAccess;
        }

        private void AllocateLayouts()
        {
            for (int i = CALLER_START_INDEX; i < _layoutCount; ++i)
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

        protected abstract uint GetConfigCount();
        protected abstract ushort GetConfigIndex<T>()
            where T : IConfig;
        protected abstract ushort GetConfigIndex(Type type);
        protected abstract ushort GetSignalIndex<T>()
            where T : ISignal;
        protected abstract SystemRegisters GetSystemRegisters();
        #endregion

        #region declarations
        public struct SystemRegisters
        {
            public ISystemRegister register;
            public ISystemAutoAttachRegister autoAttachRegister;
        }

        private struct ResizableData
        {
            public uint callerIndex;
            public IResizableCaller caller;
        }

        private struct RemoveRawIterator : IIterator<uint>
        {
            private EntityId id;
            private State state;

            public RemoveRawIterator(State state, EntityId id)
            {
                this.id = id;
                this.state = state;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref uint itemId)
            {
                state.GetCaller(itemId).RemoveRaw(id);
            }
        }
        #endregion
    }
}
