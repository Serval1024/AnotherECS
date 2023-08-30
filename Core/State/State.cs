using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Collections;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using EntityId = System.UInt32;


namespace AnotherECS.Core
{
    public unsafe abstract class State : IState, ISerializeConstructor, IDisposable, IDebugException
    {
        public bool IsDisposed { get; private set; }

        private static readonly delegate*<State, int, void> _syncCacheMethod = UnsafeUtils.ConvertToPointer(Sync);

        private GeneralConfig _generalConfig;
        private TickProvider _tickProvider;
        private Events _events;
        private Entities _entities;
        private DArrayStorage _dArrayStorage;
        //private Filters _filters;
        private Adapters _adapters;

        protected InjectContainer _injectContainer;
        private RevertAdapters _revertAdapters;
        private readonly ushort[] _componentsBufferTemp = new ushort[32];


        internal State(ref ReaderContextSerializer reader)
            => Unpack(ref reader);

        public State()
            : this(GeneralConfig.Create()) { }

        public State(in GeneralConfig general)
        {
            _generalConfig = general;
            _tickProvider = new TickProvider();
            _events = new Events(general.history.recordTickLength);

            _entities = new Entities(new EntitiesArgs(general, _tickProvider));
            _dArrayStorage = new DArrayStorage(new DArrayArgs(general, _tickProvider));
/*
#if ANOTHERECS_HISTORY_DISABLE
            _filters = new Filters(general, this, _entities);
#else
            _filters = new Filters(general, this, _entities, new FilterHistoryFactory(general.history, _history));
#endif
            _filters.Init(_adapters.Length);
*/
            _adapters = new Adapters(new IAdapter[GetComponentCount()]);

            _injectContainer = new InjectContainer(this, _dArrayStorage);

            CodeGenerationStage(general, _tickProvider);
            CodeGenerationStageNonSync(general, _tickProvider, _adapters.Gets());
            RefreshAdapters();
            ResolveDepenciesAdapters();
            RefreshOriginal();

            _revertAdapters = new RevertAdapters(_adapters.Gets());
        }

        public uint EntityCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ANOTHERECS_DEBUG
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _entities.GetCount();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId New()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_entities.TryResize())
            {
                ResizeStorages((int)_entities.GetCapacity());
            }

            return _entities.Allocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity NewEntity()
            => EntityExtensions.Pack(this, New());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            var count = _entities.GetComponents(id, _componentsBufferTemp);

            for(int i = 0; i < count; ++i)
            {
                var adapter = _adapters.GetAsEntity(_componentsBufferTemp[i]);
                if (adapter.RemoveRaw(id))
                {
                    PutAdapter(adapter, i);
                }
            }

            _entities.Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Create<T>()
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return ((IComponentFactory<T>)_adapters.Get(GetIndex<T>())).Create();
        }

#region entity data access
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>(EntityId id)
          where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            return _adapters.GetAsEntity<T>(GetIndex<T>()).IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(EntityId id, int index)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (index < 0 || index >= _entities.GetComponentCount(id))
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range component count: { _entities.GetComponentCount(id)}.");
            }
#endif
            return _adapters.GetAsEntity(_entities.GetComponent(id, index)).GetCopy(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, int index, IComponent component)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (index < 0 || index >= _entities.GetComponentCount(id))
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range component count: { _entities.GetComponentCount(id)}.");
            }
#endif
            _adapters.GetAsEntity(_entities.GetComponent(id, index)).SetUnknow(id, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (!_adapters.IsCanAsEntity<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentHasNoDataException(typeof(T));
            }
#endif
            return ref _adapters.GetAsEntity<T>(GetIndex<T>()).Read(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (!_adapters.IsCanAsEntity<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentHasNoDataException(typeof(T));
            }
#endif
            return ref _adapters.GetAsEntity<T>(GetIndex<T>()).Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(EntityId id, T data)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (!_adapters.IsCanAsEntity<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentHasNoDataException(typeof(T));
            }
#endif
            _adapters.GetAsEntity<T>(GetIndex<T>()).Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(EntityId id, ref T data)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (!_adapters.IsCanAsEntity<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentHasNoDataException(typeof(T));
            }
#endif
            _adapters.GetAsEntity<T>(GetIndex<T>()).Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, T data)
            where T : struct, IComponent
            => Add(id, ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, ref T data)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            _adapters.GetAsEntityAdd<T>(GetIndex<T>()).AddSyncVoid(id, ref data, this, _syncCacheMethod);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            return ref _adapters.GetAsEntityAdd<T>(GetIndex<T>()).AddSync(id, this, _syncCacheMethod);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void AddVoid<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            _adapters.GetAsEntity<T>(GetIndex<T>()).AddSyncVoid(id, this, _syncCacheMethod);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            var index = GetIndex<T>();
            if (_adapters.GetAsEntity<T>(index).RemoveSync(id))
            {
                Sync(index);
            }
        }
#endregion

#region single data access
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>()
          where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            return _adapters.GetAsSingle<T>(GetIndex<T>()).IsHas();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            return ref _adapters.GetAsSingle<T>(GetIndex<T>()).Read();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>()
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            return ref _adapters.GetAsSingle<T>(GetIndex<T>()).Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(T data)
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            _adapters.GetAsSingle<T>(GetIndex<T>()).Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            _adapters.GetAsSingle<T>(GetIndex<T>()).Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(T data)
            where T : struct, IShared
            => SetOrAdd(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(ref T data)
           where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            _adapters.GetAsSingle<T>(GetIndex<T>()).SetOrAdd(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : struct, IShared
        {
            ref var component = ref Add<T>();
            component = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : struct, IShared
        {
            ref var component = ref Add<T>();
            component = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            return ref _adapters.GetAsSingle<T>(GetIndex<T>()).AddSync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void AddVoid<T>()
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            _adapters.GetAsSingle<T>(GetIndex<T>()).AddSyncVoid();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : struct, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
            if (!_adapters.IsCanAsSingle<T>(GetIndex<T>()))
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
#endif
            _adapters.GetAsSingle<T>(GetIndex<T>()).RemoveSync();
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            return _entities.GetComponentCount(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetTick()
            => _tickProvider.Tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            _revertAdapters.RevertTo(_adapters.Gets(), tick);
            RefreshOriginal();
        }

        ~State()
            => Dispose(false);

        public void Dispose()        
            => Dispose(true);


        public virtual void Pack(ref WriterContextSerializer writer)
        {
#if !ANOTHERECS_HISTORY_DISABLE
            writer.WriteStruct(_generalConfig);
#endif
            writer.Pack(_tickProvider);
            _events.Pack(ref writer);
            writer.Pack(_entities);
            writer.Pack(_dArrayStorage);
            
            _adapters.Pack(ref writer);
            //writer.Pack(_filters);
        }

        public virtual void Unpack(ref ReaderContextSerializer reader)
        {
#if !ANOTHERECS_HISTORY_DISABLE
            _generalConfig = reader.ReadStruct<GeneralConfig>();
#endif
            _tickProvider = reader.Unpack<TickProvider>();
            _events.Unpack(ref reader);
            _entities = reader.Unpack<Entities>(new EntitiesArgs(_generalConfig, _tickProvider));
            _dArrayStorage = reader.Unpack<DArrayStorage>();
            
            _injectContainer = new InjectContainer(this, _dArrayStorage);
            
            _adapters.Unpack(ref reader);

            ResolveDepenciesAdapters();
            RefreshOriginal();
        }
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetFilter<T>()
            where T : Filter, new()
        {
            var mask = GetMask(typeof(T));
            if (mask.IsValide)
            {
                return _filters.Create<T>(mask);
            }
            throw new Exception();
        }
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(BaseEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            //Send(new EventContainer(_history.CurrentTick + 1, @event));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
        {
            var adapters = _adapters.Gets();
            for(int i = 1; i < adapters.Length; ++i)
            {
                if (adapters[i] is IAttachInternal attachInternal)
                {
                    attachInternal.Attach();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(ITickEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            _events.Send(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHas(EntityId id, int generation)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id) && _entities.GetGeneration(id) == generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetTypeId<T>()
            where T : struct, IComponent
            => GetIndex<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHasByTypeId(EntityId id, ushort typeId)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
            if (typeId < 0 || typeId >= _adapters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(typeId));
            }
#endif
            return _adapters.GetAsEntity(typeId).IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetGeneration(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.GetGeneration(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickStarted()
        {
            //_history.TickStarted();
            _events.TickStarted(GetTick());
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(List<ITickEvent> result)
            => GetEvent(GetTick(), result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(uint tick, List<ITickEvent> result)
            => _events.Find(tick, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetNextTickForEvent()
            => _events.NextTickForEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickFinished()
        {
            _entities.TickFinished();
            _dArrayStorage.TickFinished();
            //_filters.TickFinished();
            OnTickFinished();
        }


      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TickProvider CGGetTickProvider()
            => _tickProvider;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ushort GetIndex<T>()
            where T : struct, IComponent;
        protected abstract void OnTickFinished();
        protected abstract void CodeGenerationStage(in GeneralConfig general, TickProvider tickProvider);
        protected abstract void CodeGenerationStageNonSync(in GeneralConfig general, TickProvider tickProvider, IAdapterReference[] adapters);
        protected abstract int GetComponentCount();
        protected abstract void GetAdapters(IAdapterReference[] adapters);
        protected abstract void PutAdapters(IAdapterReference[] adapters);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void PutAdapter(IAdapterReference adapter, int index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IInjectMethodsReference[] GetInjects();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Mask GetMask(Type type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sync(State state, int index)
        {
            state.Sync(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Sync(int index)
        {
            PutAdapter(_adapters.Get(index), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshAdapters()
        {
            GetAdapters(_adapters.Gets());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshOriginal()
        {
            PutAdapters(_adapters.Gets());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeStorages(int capacity)
        {
            _adapters.Resize(capacity);
            RefreshOriginal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResolveDepenciesAdapters()
        {
            var injects = GetInjects();
            var adapters = _adapters.Gets();
            for (int i = 1; i < adapters.Length; ++i)
            {
                ResolveDepenciesAdapter(adapters[i], injects);
            }
        }

        private void ResolveDepenciesAdapter<T>(T adapter, IInjectMethodsReference[] injects)
            where T : IAdapter
        {
#if ANOTHERECS_DEBUG
            adapter.SetState(this);
#endif
            if (adapter is IEntityAdapter entityAdapter)
            {
                //entityAdapter.BindExternal(_entities, _filters, ref _adapters);
                entityAdapter.BindExternal(_entities, null, ref _adapters);
            }

            if (adapter is IStateBindExternalInternal stateBindExternalInternal)
            {
                stateBindExternalInternal.BindExternal(this);
            }

            if (adapter is IInjectSupportInternal injectSupportInternal)
            {
                injectSupportInternal.BindInject(ref _injectContainer, injects);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _adapters.Recycle();
                //_history.Recycle();

                _adapters.Dispose();
                //_history.Dispose();
                _dArrayStorage.Dispose();

                IsDisposed = true;

                GC.SuppressFinalize(this);
            }
        }
    }
}

