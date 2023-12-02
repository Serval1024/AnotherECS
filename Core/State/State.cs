using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AnotherECS.Unsafe;
using AnotherECS.Serializer;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public abstract unsafe class State : BaseState, ISerializeConstructor
    {
        #region const
        private const uint CORE_LAYOUT_COUNT = 5;
        private const int COMPONENT_ENTITY_MAX = 32;
        #endregion
        #region data
        private GlobalDepencies* _depencies;
        private NArray<UnmanagedLayout> _layouts;
        private uint _layoutCount;

        private ICaller[] _callers;

        private Events _events;
        #endregion

        #region data cache
        private EntitiesCaller _entities;
        private DArrayCaller _dArray;
        private ArchetypeCaller _archetype;
        private NArray<ushort> _temporaryIndexes;

        private bool[] _isCustomSerializeCallers;
        private ITickFinishedCaller[] _tickFinishedCallers;  //TODO SER MTHREAD
        private IResizableCaller[] _resizableCallers;  //TODO SER MTHREAD
        private IRevertCaller[] _revertCallers;
        #endregion

        #region construct & destruct
        public State()
            : this(StateConfig.Create()) { }

        public State(in StateConfig config)
        {
            _layoutCount = 1;
            _layouts = new NArray<UnmanagedLayout>(GetLayoutCount());
            _callers = new ICaller[GetLayoutCount()];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();

            _depencies->config = config;
            _depencies->tickProvider = new TickProvider();

            _events = new Events(config.history.recordTickLength);

            CommonInit();
            AllocateLayouts();

            _temporaryIndexes = GetTemporaryIndexes();
        }

        internal State(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
        }

        protected override void OnDispose()
        {
            for (int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i] is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            for(int i = 1; i < _layoutCount; ++i)
            {
                _layouts.GetRef(i).Dispose();
            }

            _layouts.Dispose();
            UnsafeMemory.DisposeDeallocate(ref _depencies);
            _temporaryIndexes.Dispose();
        }
        #endregion

        #region serialization
        public void Pack(ref WriterContextSerializer writer)
        {
            for(int i = 1; i < _layoutCount; ++i)
            {
                if (_isCustomSerializeCallers[i])
                {
                    ((ISerialize)_callers[i]).Pack(ref writer);
                }
                else
                {
                    LayoutSerializer.PackBlittable(ref writer, ref _layouts.GetRef(i));
                }
            }

            _depencies->Pack(ref writer);
            _events.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _layoutCount = 1;

            _layouts = new NArray<UnmanagedLayout>(GetLayoutCount());
            _callers = new ICaller[GetLayoutCount()];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();
            _depencies->Unpack(ref reader);
            _events.Unpack(ref reader);

            CommonInit();

            for (int i = 1; i < _layoutCount; ++i)
            {
                if (_isCustomSerializeCallers[i])
                {
                    ((ISerialize)_callers[i]).Unpack(ref reader);
                }
                else
                {
                    LayoutSerializer.UnpackBlittable(ref reader, ref _layouts.GetRef(i));
                }
            }
        }

        private void CommonInit()
        {
            BindingCodeGenerationStage(_depencies->config);

            _depencies->componentTypesCount = GetComponentCount();
            _entities = EntitiesCaller.LayoutInstaller.Install(this);
            _dArray = DArrayCaller.LayoutInstaller.Install(this);
            _archetype = ArchetypeCaller.LayoutInstaller.Install(this);

            _depencies->entities = _entities;
            _depencies->dArray = _dArray;
            _depencies->archetype = _archetype;
            _depencies->injectContainer = new InjectContainer(_dArray);
            _depencies->filters = new Filters(_archetype, 32);

            _isCustomSerializeCallers = _callers.Skip(1).Select(p => p.IsSerialize).Prepend(false).ToArray();
            _tickFinishedCallers = _callers.Skip(1).Where(p => p.IsTickFinished && p is ITickFinishedCaller).Cast<ITickFinishedCaller>().ToArray();
            _resizableCallers = _callers.Skip(1).Where(p => p.IsResizable && p is IResizableCaller).Cast<IResizableCaller>().ToArray();
            _revertCallers = _callers.Skip(1).Where(p => p.IsRevert && p is IRevertCaller).Cast<IRevertCaller>().ToArray();
        }
        #endregion

        #region init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
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
                return _entities.GetCount();
            }
        }

        public bool IsHas(uint id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId New()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_entities.TryResizeDense())
            {
                ResizeStorages(_entities.GetCapacity());
            }
            var id = _entities.Allocate();
            _depencies->archetype.Add(id);
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeStorages(uint capacity)
        {
            for(int i = 0; i < _resizableCallers.Length; ++i)
            {
                _resizableCallers[i].Resize(capacity);
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
            if (_archetype.GetCount(_entities.ReadArchetypeId(id)) > COMPONENT_ENTITY_MAX)
            {
                throw new Exceptions.ReachedLimitComponentOnEntityException(COMPONENT_ENTITY_MAX);
            }
#endif
            var componentIds = stackalloc uint[COMPONENT_ENTITY_MAX];
            var archetypeId = _entities.ReadArchetypeId(id);
            var count = _archetype.GetItemIds(archetypeId, componentIds, COMPONENT_ENTITY_MAX);

            for(int i = 0; i < count; ++i)
            {
                GetCaller(componentIds[i]).RemoveRaw(id);
            }

            _archetype.Remove(archetypeId, id);
            _entities.Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _archetype.GetCount(_entities.ReadArchetypeId(id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHas(EntityId id, ushort generation)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id) && _entities.ReadGeneration(id) == generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetGeneration(EntityId id)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _entities.ReadGeneration(id);
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
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller(_archetype.GetItemId(_entities.ReadArchetypeId(id), index)));
#endif
            return GetCaller(_archetype.GetItemId(_entities.ReadArchetypeId(id), index)).GetCopy(id);
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
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller(_archetype.GetItemId(_entities.ReadArchetypeId(id), index)));
#endif
            GetCaller(_archetype.GetItemId(_entities.ReadArchetypeId(id), index)).Set(id, component);
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
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            return GetCaller<T>().IsHas(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(T data)
            where T : unmanaged, IShared
            => SetOrAdd(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(ref T data)
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            GetCaller<T>().SetOrAdd(0, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : unmanaged, IShared
            => Add(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Add(0, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Add(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T>().Remove(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Read(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Get<T>()
            where T : unmanaged, IShared
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T>().Get(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : unmanaged, IShared
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
        internal ref readonly IdCollection GetEntitiesByArchetype(uint archetypeId)
        { 
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return ref _archetype.GetIdCollection(archetypeId);
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
        public void RevertTo(uint tick) //TODO SER inter
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            foreach(var revert in _revertCallers)
            {
                UnityEngine.Debug.Log(revert.GetType().FullName);
                revert.RevertTo(tick, this);
            }

            Tick = tick;
        }

        internal NArray<ushort> GetTemporary()
            => _temporaryIndexes;
        private NArray<ushort> GetTemporaryIndexes()
        {
            using var list = new NList<ushort>(32);

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
            => UnsafeUtils.As<ICaller, ICaller<T>>(ref _callers[index]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller<T> GetCaller<T>()
            where T : unmanaged, IComponent
            => GetCaller<T>(GetIndex<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller GetCaller(uint index)
            => _callers[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal UCaller AddLayout<UCaller, TSparse, TDense, TDenseIndex, TTickData>(ComponentFunction<TDense> componentFunction = default)
            where UCaller : struct, ICallerReference
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (_layoutCount == GetLayoutCount())
            {
                throw new InvalidOperationException();
            }
#endif
            var layout = (UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>*)_layouts.GetPtr(_layoutCount);
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
                throw new InvalidOperationException();
            }
#endif
            var icaller = (ICaller)caller;
            _callers[_layoutCount] = icaller;

            icaller.Config(_layouts.GetPtr(_layoutCount), _depencies, (ushort)_layoutCount, this);

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
    }
}

