using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Collections;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using EntityId = System.Int32;

namespace AnotherECS.Core
{
    public unsafe abstract class State : IState, IDisposable, ISerializeConstructor, IDebugException
    {
        public bool IsDisposed { get; private set; }

        private static readonly delegate*<State, int, void> _syncCache = UnsafeUtils.ConvertToPointer(Sync);

        private Histories _history;
        private Entities _entities;
        private Filters _filters;
        private TickProvider _tickProvider;
        private DArrayStorage _dArrayStorage;
        private Adapters _adapters;
        private Events _events;
        protected InjectContainer _injectContainer;
#if !ANOTHERECS_HISTORY_DISABLE
        private GeneralConfig _generalConfig;
#endif
        internal State(ref ReaderContextSerializer reader)
            => Unpack(ref reader);

        public State()
            : this(GeneralConfig.Create()) { }

        public State(in GeneralConfig general)
        {
            _tickProvider = new TickProvider();
            _history = new Histories(general.history, _tickProvider);
            _events = new Events(general.history.recordTickLength);

#if ANOTHERECS_HISTORY_DISABLE
            _entities = new Entities(general);
#else
            _generalConfig = general;
            var entitiesHistory = new EntitiesHistory(general.history, _tickProvider);
            _entities = new Entities(general, entitiesHistory);
            entitiesHistory.SetSubject(_entities);
#endif

#if ANOTHERECS_HISTORY_DISABLE
            _filters = new Filters(general, this, _entities);
#else
            _filters = new Filters(general, this, _entities, new FilterHistoryFactory(general.history, _history));
#endif
            _adapters = new Adapters(new IAdapter[GetComponentCount()]);
            _filters.Init(_adapters.Length);

#if ANOTHERECS_HISTORY_DISABLE
            _dArrayStorage = new DArrayStorage(general.flexArrayCapacity);
#else
            var dArrayHistory = new DArrayHistory(general.history, _tickProvider);
            _dArrayStorage = new DArrayStorage(general.dArrayCapacity, dArrayHistory);
            dArrayHistory.SetSubject(_dArrayStorage);
#endif

            _injectContainer = new InjectContainer(this, _dArrayStorage);
#if !ANOTHERECS_HISTORY_DISABLE
            _history.RegisterChild(entitiesHistory);
            _history.RegisterChild(dArrayHistory);
#endif
            CodeGenerationStage(general, _tickProvider);
            CodeGenerationStageNonSync(general, _tickProvider, _adapters.Gets());
            RefreshAdapters();
        }

        public int EntityCount
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
            var id = _entities.Allocate(out int newSize);

            if (newSize != -1)
            {
                PoolResize(newSize);
                RefreshAdapters();
                _filters.ResizeSparseIndex(newSize);
            }

            return id;
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
            var offset = _entities.GetOffsetSegment(id);
            var componentCount = _entities.GetRef(offset + Entities.IndexOffset.ComponentCount);

            _entities.Deallocate(id, offset);

            if (componentCount > 0)
            {
                for (int i = offset + Entities.IndexOffset.BeginComponent, iMax = offset + Entities.IndexOffset.BeginComponent + componentCount; i < iMax; ++i)
                {
                    var adapter = _adapters.GetAsEntity(_entities[i]);
                    if (adapter.RemoveRaw(id))
                    {
                        PutAdapter(adapter, i);
                    }
                }
            }            
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
            _adapters.GetAsEntityAdd<T>(GetIndex<T>()).AddSyncVoid(id, ref data, this, _syncCache);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            return ref _adapters.GetAsEntityAdd<T>(GetIndex<T>()).AddSync(id, this, _syncCache);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void AddVoid<T>(EntityId id)
            where T : struct, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfInvalide(this, id);
#endif
            _adapters.GetAsEntity<T>(GetIndex<T>()).AddSyncVoid(id, this, _syncCache);
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
            _history.RevertTo(tick, _adapters.Gets());
            RefreshOriginal();
        }

        ~State()
            => Dispose(false);

        public void Dispose()        
            => Dispose(true);


        public virtual void Pack(ref WriterContextSerializer writer)
        {
            _events.Pack(ref writer);
            
#if !ANOTHERECS_HISTORY_DISABLE
            writer.WriteStruct(_generalConfig);
#endif
            writer.Pack(_tickProvider);
            writer.Pack(_history);
            writer.Pack(_entities);
            writer.Pack(_dArrayStorage);
            _adapters.Pack(ref writer);
            writer.Pack(_filters);
        }

        public virtual void Unpack(ref ReaderContextSerializer reader)
        {
            _events.Unpack(ref reader);
            
#if !ANOTHERECS_HISTORY_DISABLE
            _generalConfig = reader.ReadStruct<GeneralConfig>();
#endif
            _tickProvider = reader.Unpack<TickProvider>();
            _history = reader.Unpack<Histories>(_tickProvider);
            
#if ANOTHERECS_HISTORY_DISABLE
            _entities = reader.Unpack<Entities>();
            _dArrayStorage = reader.Unpack<DArrayStorage>();
#else
            _entities = reader.Unpack<Entities>(_history.GetChild<EntitiesHistory>());
            _history.GetChild<EntitiesHistory>().SetSubject(_entities);

            _dArrayStorage = reader.Unpack<DArrayStorage>(_history.GetChild<DArrayHistory>());
            _history.GetChild<DArrayHistory>().SetSubject(_dArrayStorage);
#endif
            _history.BindExternal(this);


            _injectContainer = new InjectContainer(this, _dArrayStorage);
            
            _adapters.Unpack(ref reader);
#if ANOTHERECS_HISTORY_DISABLE
            _filters = reader.Unpack<Filters>(this, _entities);
#else
            _filters = reader.Unpack<Filters>(this, _entities, new FilterHistoryFactory(_generalConfig.history, _history));
#endif
            _filters.AllFilterRebind(this, GetMask, p => _history.GetChild<FilterHistory>(r => r.SubjectId == p.Id));
            _filters.Init(_adapters.Length);

            var adapters = _adapters.Gets();
            for (int i = 0; i < adapters.Length; ++i)
            {
                var adapter = adapters[i];

                if (adapter is IEntityAdapter entityAdapter)
                {
                    entityAdapter.BindExternal(_entities, _filters, ref _adapters);
#if ANOTHERECS_DEBUG
                    entityAdapter.SetState(this);
#endif
                }

                if (adapter is IHistoryBindExternalInternal historyBindExternalInternal)
                {
                    historyBindExternalInternal.BindExternal(_history.GetChild<PoolHistory>(r => r.SubjectId == i));
                }

                if (adapter is IStateBindExternalInternal stateBindExternalInternal)
                {
                    stateBindExternalInternal.BindExternal(this);
                }

                var injects = GetInjects();
                if (adapter is IInjectSupportInternal injectSupportInternal)
                {
                    injectSupportInternal.BindInject(ref _injectContainer, injects);
                }
            }

            for (int i = 0; i < adapters.Length; ++i)
            {
                if (adapters[i] is IAttachInternal attachInternal)
                {
                    attachInternal.Attach();
                }
            }

            RefreshOriginal();
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(BaseEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            Send(new EventContainer(_history.CurrentTick + 1, @event));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
        {
            foreach(var adapter in _adapters.Gets())
            {
                if (adapter is IAttachInternal attachInternal)
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
            _history.TickStarted();
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
            _filters.TickFinished();
            OnTickFinished();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T CGInitAdapter<T>(T data)
        {
            var adapter = data as IAdapter;

            if (adapter is IEntityAdapter entityAdapter)
            {
                entityAdapter.BindExternal(_entities, _filters, ref _adapters);
            }
            if (adapter is IStateBindExternalInternal stateBindExternalInternal)
            {
                stateBindExternalInternal.BindExternal(this);
            }
#if ANOTHERECS_DEBUG
            adapter.SetState(this);
#endif
            return (T)adapter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T CGInitHistory<T>(T history)
            where T : IHistory
        {
            _history.RegisterChild(history);
            if (history is IStateBindExternalInternal stateBindExternalInternal)
            {
                stateBindExternalInternal.BindExternal(this);
            }
            return history;
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
         => state.Sync(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Sync(int index)
            => PutAdapter(_adapters.Get(index), index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshAdapters()
            => GetAdapters(_adapters.Gets());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshOriginal()
            => PutAdapters(_adapters.Gets());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PoolResize(int capacity)
        {
            _adapters.Resize(capacity);
            RefreshOriginal();
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _adapters.Recycle();
                _history.Recycle();

                _adapters.Dispose();
                _history.Dispose();
                _dArrayStorage.Dispose();

                IsDisposed = true;

                GC.SuppressFinalize(this);
            }
        }
    }
}

