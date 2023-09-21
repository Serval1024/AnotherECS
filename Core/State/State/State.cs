using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AnotherECS.Unsafe;
using AnotherECS.Serializer;
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
        #region data
        private GlobalDepencies* _depencies;

        private UnmanagedLayout* _layoutPtr;
        private uint _layoutCount;

        private ICaller[] _callers;
        #endregion

        #region cache
        private EntitiesCaller _entities;

        private IResizableCaller[] _resizableCallers;
        private readonly ushort[] _componentsBufferTemp;
        #endregion

        #region construct & destruct
        public State()
            : this(GeneralConfig.Create()) { }

        public State(in GeneralConfig config)
        {
            _componentsBufferTemp = new ushort[64];

            _layoutCount = 1;
            _layoutPtr = (UnmanagedLayout*)UnsafeMemory.Allocate<UnmanagedLayout<UnmanagedLayout.Mock>>(GetComponentCount() + 1);
            _callers = new ICaller[GetComponentCount() + 1];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();

            _entities = AddLayout<EntitiesCaller, EntityHead>();
            _depencies->config = config;
            _depencies->tickProvider = new TickProvider();
            _depencies->entities = _entities;

            BindingCodeGenerationStage(config);

            _resizableCallers = _callers.Where(p => p is IResizableCaller).Cast<IResizableCaller>().ToArray();
            AllocateLayouts();
        }

        internal State(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
        }


        public override void OnDispose()
        {
            for (int i = 0; i < _callers.Length; ++i)
            {
                if (_callers[i] is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            for(int i = 1; i < _layoutCount; ++i)
            {
                (_layoutPtr + i)->Dispose();
            }

            UnsafeMemory.Deallocate(ref _layoutPtr);
            UnsafeMemory.Deallocate(ref _depencies);
        }
        #endregion

        #region serialization
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(new ArrayPtr(_layoutPtr, (uint)sizeof(UnmanagedLayout) * _layoutCount, _layoutCount));

            _depencies->Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _layoutCount = 1;

            _layoutPtr = (UnmanagedLayout*)reader.Unpack<ArrayPtr>().GetPtr();

            _callers = new ICaller[GetComponentCount() + 1];

            _depencies = UnsafeMemory.Allocate<GlobalDepencies>();
            _depencies->Unpack(ref reader);

            _entities = AddLayout<EntitiesCaller, EntityHead>();
            _depencies->entities = _entities;

            BindingCodeGenerationStage(_depencies->config);

            _resizableCallers = _callers.Where(p => p is IResizableCaller).Cast<IResizableCaller>().ToArray();
        }
        #endregion

        #region init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FirstStartup()
        {
            for (int i = 1; i < _callers.Length; ++i)
            {
                if (_callers[i] is IAttachInternal attachInternal)
                {
                    attachInternal.Attach();
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
            ExceptionHelper.ThrowIfExists<T>(this, id, GetCaller<T>());
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
            ExceptionHelper.ThrowIfDontExists(this, id, index, 0, GetCaller<ICaller>(_entities.GetComponentCount(id)));
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
            ExceptionHelper.ThrowIfDontExists(this, id, index, 0, GetCaller<ICaller>(_entities.GetComponentCount(id)));
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
            //_events.TickStarted(GetTick());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TickFinished()
        {
            _entities.TickFinished();
            //_dArrayStorage.TickFinished();
            //_filters.TickFinished();
            OnTickFinished();
        }
        #endregion

        #region ticks api
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(BaseEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            //Send(new EventContainer(_history.CurrentTick + 1, @event));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Send(ITickEvent @event)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            //_events.Send(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(List<ITickEvent> result)
            => GetEvent(GetTick(), result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetEvent(uint tick, List<ITickEvent> result)
        //=> _events.Find(tick, result);
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetTick()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint GetNextTickForEvent()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RevertTo(uint tick)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            //_revertAdapters.RevertTo(_adapters.Gets(), tick);
        }
        #endregion

        #region helpers
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
            => UnsafeUtility.As<ICaller, U>(ref _callers[index]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private U GetCaller<T, U>()
            where T : unmanaged, IComponent
            where U : ICaller
#if ANOTHERECS_DEBUG
            => (U)_callers[GetIndex<T>()];
#else
            => UnsafeUtility.As<ICaller, U>(ref _callers[GetIndex<T>()]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UCaller AddLayout<UCaller, TComponent>(ComponentFunction<TComponent> componentFunction = default)    //TODO SER hide interface
            where UCaller : unmanaged, ICaller
            where TComponent : unmanaged
        {
#if ANOTHERECS_DEBUG
            if (_layoutCount == GetComponentCount() + 1)
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

        private void AllocateLayouts()
        {
            for (int i = 1; i < _layoutCount; ++i)
            {
                _callers[i].AllocateLayout();
            }
        }

        protected void UpdateFastAccess<T>(T* fastAccess)
            where T : unmanaged
        {
            ((FastAccess*)fastAccess)->layoutPtr = _layoutPtr;
        }
        #endregion

        #region codegen & abstract
        protected abstract void BindingCodeGenerationStage(in GeneralConfig config);
        protected abstract uint GetComponentCount();
        protected abstract ushort GetIndex<T>()
            where T : struct, IComponent;
        protected abstract void OnTickFinished();
        #endregion
    }
}

