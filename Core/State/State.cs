using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AnotherECS.Unsafe;
using AnotherECS.Serializer;
using EntityId = System.UInt32;
using AnotherECS.Core.Actions;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public abstract unsafe class State : BaseState, ISerializeConstructor
    {
        #region data
        private GlobalDepencies* _depencies;

        private UnmanagedLayout* _layoutPtr;
        private uint _layoutCount;

        private ICaller[] _callers;

        private Events _events;
        #endregion

        #region data cache
        private EntitiesCaller _entities;
        private DArrayCaller _dArray;

        private bool[] _isCustomSerializeCallers;
        private ITickFinishedCaller[] _tickFinishedCallers;  //TODO SER MTHREAD
        private IResizableCaller[] _resizableCallers;  //TODO SER MTHREAD
        private IRevertCaller[] _revertCallers;
        private readonly ushort[] _componentsBufferTemp;
        #endregion

        #region construct & destruct
        public State()
            : this(WorldConfig.Create()) { }

        public State(in WorldConfig config)
        {
            _componentsBufferTemp = new ushort[64];

            _layoutCount = 1;
            _layoutPtr = UnsafeMemory.Allocate<UnmanagedLayout>(GetLayoutCount());
            _callers = new ICaller[GetLayoutCount()];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();

            _depencies->config = config;
            _depencies->tickProvider = new TickProvider();

            _events = new Events(config.history.recordTickLength);

            CommonInit();
            AllocateLayouts();
        }

        internal State(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
        }

        public override void OnDispose()
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
                _layoutPtr[i].Dispose();
            }

            UnsafeMemory.Deallocate(ref _layoutPtr);
            UnsafeMemory.Deallocate(ref _depencies);
        }
        #endregion

        #region serialization
        public void Pack(ref WriterContextSerializer writer)
        {
            for(int i = 1; i < GetLayoutCount(); ++i)
            {
                if (_isCustomSerializeCallers[i])
                {
                    ((ISerialize)_callers[i]).Pack(ref writer);
                }
                else
                {
                    LayoutSerializer.PackBlittable(ref writer, ref _layoutPtr[i]);
                }
            }

            _depencies->Pack(ref writer);
            _events.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _layoutCount = 1;

            _layoutPtr = UnsafeMemory.Allocate<UnmanagedLayout>(GetLayoutCount());
            _callers = new ICaller[GetLayoutCount()];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();
            _depencies->Unpack(ref reader);
            _events.Unpack(ref reader);

            CommonInit();

            for (int i = 1; i < GetLayoutCount(); ++i)
            {
                if (_isCustomSerializeCallers[i])
                {
                    ((ISerialize)_callers[i]).Unpack(ref reader);
                }
                else
                {
                    LayoutSerializer.UnpackBlittable(ref reader, ref _layoutPtr[i]);
                }
            }
        }

        private void CommonInit()
        {
            BindingCodeGenerationStage(_depencies->config);

            _entities = AddLayout<EntitiesCaller, EntityHead>();
            _dArray = AddLayout<DArrayCaller, DArrayCaller.Container>();
            _depencies->entities = _entities;
            _depencies->dArray = _dArray;
            _depencies->injectContainer = new InjectContainer(_dArray);

            _isCustomSerializeCallers = _callers.Select(p => p is ISerialize).ToArray();
            _tickFinishedCallers = _callers.Where(p => p is ITickFinishedCaller).Cast<ITickFinishedCaller>().ToArray();
            _resizableCallers = _callers.Where(p => p is IResizableCaller).Cast<IResizableCaller>().ToArray();
            _revertCallers = _callers.Where(p => p is IRevertCaller).Cast<IRevertCaller>().ToArray();
        }
        #endregion

        #region init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
        {
            for (int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i] is IAttachCaller callerAttach)
                {
                    callerAttach.Attach();
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
#if ANOTHERECS_DEBUG
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _entities.GetCount();
            }
        }

        public bool IsHas(uint id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId New()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (_entities.TryResizeDense())
            {
                ResizeStorages(_entities.GetCapacity());
            }
            return _entities.Allocate();
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
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            var count = _entities.GetComponents(id, _componentsBufferTemp);

            for(int i = 0; i < count; ++i)
            {
                GetCaller<IMultiCaller>(_componentsBufferTemp[i])
                    .Remove(id);
            }

            _entities.Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _entities.GetComponentCount(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsHas(EntityId id, ushort generation)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return _entities.IsHas(id) && _entities.GetGeneration(id) == generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort GetGeneration(EntityId id)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id);
#endif
            return _entities.GetGeneration(id);
        }
        #endregion

        #region multi component
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Create<T>()
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            return GetCaller<T, IMultiCaller<T>>().Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>(EntityId id)
          where T : unmanaged, IComponent
        {

#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfNotMultiAccess(this, id, GetCaller<T>());
#endif
            return GetCaller<T, IMultiCaller<T>>().IsHas(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, T data)
            where T : unmanaged, IComponent
            => Add(id, ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            GetCaller<T, IMultiCaller<T>>().Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T, IMultiCaller<T>>().Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfExists(this, id, GetCaller<T>());
#endif
            Add<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            GetCaller<T, IMultiCaller<T>>().Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(EntityId id, int index)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller<ICaller>(_entities.GetComponentCount(id)));
#endif
            return GetCaller<IMultiCaller>(_entities.GetComponentCount(id)).GetCopy(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T, IMultiCaller<T>>().Read(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(EntityId id)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            return ref GetCaller<T, IMultiCaller<T>>().Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, int index, IComponent component)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, index, Count(id), GetCaller<ICaller>(_entities.GetComponentCount(id)));
#endif
            GetCaller<IMultiCaller>(_entities.GetComponent(id, index)).Set(id, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(EntityId id, ref T data)
            where T : unmanaged, IComponent
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, id, GetCaller<T>());
#endif
            GetCaller<T, IMultiCaller<T>>().Set(id, ref data);
        }
        #endregion

        #region single component
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<T>()
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            return GetCaller<T, ISingleCaller<T>>().IsHas();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(T data)
            where T : unmanaged, IShared
            => SetOrAdd(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrAdd<T>(ref T data)
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfNotSingleAccess(this, GetCaller<T>());
#endif
            GetCaller<T, ISingleCaller<T>>().SetOrAdd(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : unmanaged, IShared
            => Add(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            GetCaller<T, ISingleCaller<T>>().Add(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T, ISingleCaller<T>>().Add();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T, ISingleCaller<T>>().Remove();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T, ISingleCaller<T>>().Read();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Get<T>()
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            return ref GetCaller<T, ISingleCaller<T>>().Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : unmanaged, IShared
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDontExists(this, GetCaller<T>());
#endif
            GetCaller<T, ISingleCaller<T>>().Set(ref data);
        }
        #endregion

        #region events
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickStarted()
        {
            //_history.TickStarted();
            _events.TickStarted(GetTick());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickFinished()
        {
            foreach (var tickFinished in _tickFinishedCallers)
            {
                tickFinished.TickFinished();
            }

            _entities.TickFinished();
            _dArray.TickFinished();
            //_filters.TickFinished();
            OnTickFinished();
        }
        #endregion

        #region ticks api
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetTick()
            => _depencies->tickProvider.tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(BaseEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            Send(new EventContainer(_depencies->tickProvider.tick + 1, @event));
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
        internal void GetEvent(List<ITickEvent> result)
            => GetEvent(GetTick(), result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(uint tick, List<ITickEvent> result)
            => _events.Find(tick, result);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetNextTickForEvent()
            => _events.NextTickForEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RevertTo(uint tick)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            foreach(var revert in _revertCallers)
            {
                revert.RevertTo(tick, this);
            }
            _dArray.RevertFinished();
        }
        #endregion

        #region helpers
        private uint GetLayoutCount()
            => GetComponentCount() + 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICaller<T> GetCaller<T>()
            where T : unmanaged, IComponent
            => (ICaller<T>)_callers[GetIndex<T>()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private U GetCaller<U>(int index)
            where U : ICaller
#if ANOTHERECS_DEBUG
            => (U)_callers[index];
#else
            => Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<ICaller, U>(ref _callers[index]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private U GetCaller<T, U>()
            where T : unmanaged, IComponent
            where U : ICaller
#if ANOTHERECS_DEBUG
            => (U)_callers[GetIndex<T>()];
#else
            => Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<ICaller, U>(ref _callers[GetIndex<T>()]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UCaller AddLayout<UCaller, TComponent>(ComponentFunction<TComponent> componentFunction = default)
            where UCaller : struct, ICaller
            where TComponent : unmanaged
        {
#if ANOTHERECS_DEBUG
            if (_layoutCount == GetLayoutCount())
            {
                throw new InvalidOperationException();
            }
#endif
            var layout = (UnmanagedLayout<TComponent>*)(_layoutPtr + _layoutCount);
            layout->componentFunction = componentFunction;

            var caller = (ICaller<TComponent>)default(UCaller);
            _callers[_layoutCount] = caller;
            
            caller.Config(_layoutPtr + _layoutCount, _depencies, (ushort)_layoutCount, this);

            ++_layoutCount;

            return (UCaller)caller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EFastAccess CreateFA<TComponent, UCaller, EFastAccess>()
            where TComponent : IComponent
            where UCaller : ICaller
            where EFastAccess : struct, IFastAccess<UCaller>
        {
            EFastAccess fastAccess = default;
#if ANOTHERECS_DEBUG
            fastAccess.Config((UCaller)_callers[GetIndex<TComponent>()]);
#else
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<ICaller, UCaller>(ref _callers[GetIndex<TComponent>()]);
#endif
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
        protected abstract void BindingCodeGenerationStage(in WorldConfig config);
        protected abstract uint GetComponentCount();
        protected abstract ushort GetIndex<T>()
            where T : IComponent;
        protected abstract void OnTickFinished();
        #endregion
    }
}

