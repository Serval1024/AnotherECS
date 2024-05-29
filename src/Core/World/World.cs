using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Debug.Diagnostic")]
namespace AnotherECS.Core
{
    public class World : BDisposable, IWorldExtend, IWorldLiveLoop
    {
        public string Name { get; set; }
        public uint Id { get; private set; }
        public uint RequestTick { get; private set; }
        public uint CurrentTick => _worldData.CurrentTick;

        public State State
        { 
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _worldData.State;
            }
            set
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                if (_worldData.State != value)
                {
                    TryDisposeState();
                    _worldData.State = value;
                    TryApplyState();
                }
            }
        }

        public WorldData WorldData
        {
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _worldData;
            }
            set
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                if (_worldData != value)
                {
                    TryDisposeWorldData();
                    _worldData = value;

                    if (_isInit)
                    {
                        TryApplyWorldData();
                    }
                }
            }
        }


        private bool _isInit;

        

        private WorldData _worldData;
        private ISystem[] _flatSystemsCache;
        private readonly LoopProcessing _loopProcessing;
        private readonly WorldSignals _signals;

        private bool IsStartup => _worldData.State != null && _worldData.State.IsCalledStartup;

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private IWorldStatistic _statistic;
#endif

        public World(WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(new SystemGroup(), null, SystemProcessingFactory.Create(threadingLevel)) { }

        public World(ISystem system, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(new SystemGroup(system), null, SystemProcessingFactory.Create(threadingLevel)) { }

        public World(IGroupSystem systems, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(systems, null, SystemProcessingFactory.Create(threadingLevel)) { }

        public World(ISystem system, State state, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(new SystemGroup(system), state, SystemProcessingFactory.Create(threadingLevel)) { }

        public World(IGroupSystem systems, State state, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(systems, state, SystemProcessingFactory.Create(threadingLevel)) { }

        public World(IEnumerable<ISystem> systems, State state, ISystemProcessing systemProcessing = default)
        {
            _worldData = new WorldData(systems, state);
            _loopProcessing = new LoopProcessing(systemProcessing);
            _signals = WorldSignals.Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasConfig<T>()
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_worldData.IsEmpty)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            return _worldData.State.IsHasConfig<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World AddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_worldData.IsEmpty)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            _worldData.State.AddConfig(data);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World SetOrAddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_worldData.IsEmpty)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            _worldData.State.SetOrAddConfig(data);
            return this;
        }

        public void Init()
        {
            ExceptionHelper.ThrowIfDisposed(this);

            if (_isInit)
            {
                throw new InvalidOperationException($"The world has already been initialized.");
            }

            if (_worldData.IsEmpty)
            {
                throw new InvalidOperationException($"The state is not assigned.");
            }

            InitInternal();
        }

        public void Startup()
        {
            ExceptionHelper.ThrowIfWorldRaw(this, _isInit);
            if (_worldData.IsEmpty)
            {
                throw new InvalidOperationException($"The state not assigned.");
            }

            _loopProcessing.StateStartup();
        }

        public void Tick()
        {
            Tick(1u);
        }

        public void Tick(uint tickCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
#endif
            if (_worldData.IsOneGateCallCreate)
            {
                _worldData.IsOneGateCallCreate = false;
                _loopProcessing.Create();
            }

#if !ANOTHERECS_HISTORY_DISABLE
            uint revertTick = _loopProcessing.TryRevertTo(RequestTick, _worldData.State.GetNextTickForEvent());
#else 
            const uint revertTick = 0;
#endif
            if (revertTick != 0)
            {
                for (int i = 0; i < revertTick; ++i)
                {
                    _loopProcessing.Tick();
                }
                _loopProcessing.RevertFinished();
            }

            if (tickCount != 0)
            {
                RequestTick += tickCount;

                for (int i = 0; i < tickCount; ++i)
                {
                    _loopProcessing.Tick();
                }
            }
        }

        public void DispatchSignals()
        {
            _signals.DispatchSignals(ref _worldData);
        }

        public void RevertTo(uint tick)
        {
            _loopProcessing.RevertTo(tick);
        }

        public void Destroy()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
            if (!_worldData.IsOneGateCallDestroy)
            {
                throw new InvalidOperationException($"The world has already been destroy.");
            }
#endif
            _worldData.IsOneGateCallDestroy = false;
            _loopProcessing.Destroy();
        }

        public void UpdateFromMainThread()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
#endif
            _loopProcessing.CallFromMainThread();
        }

        public void SendEvent(IEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
#endif
            _worldData.State.Send(@event);
        }

        public void SendEvent(ITickEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
#endif
            _worldData.State.Send(@event);
        }

        public void AddSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            _signals.AddSignal(receiver);
        }

        public void RemoveSignal<TSignal>(ISignalReceiver<TSignal> receiver)
           where TSignal : ISignal
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            _signals.RemoveSignal(receiver);
        }

        public bool IsBusy()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, _isInit, IsStartup);
#endif
            return _loopProcessing.IsBusy();
        }

        public void Wait()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (!_isInit || _worldData.IsEmpty)
            {
                return;
            }

            _loopProcessing.Wait();
        }

        public TModuleData GetModuleData<TModuleData>(uint id)
            where TModuleData : IModuleData
            => _worldData.State.GetModuleData<TModuleData>(id);

        public void SetModuleData<TModuleData>(uint id, TModuleData data)
            where TModuleData : IModuleData
        {
            _worldData.State.SetModuleData(id, data);
        }

        public WorldStatisticData GetStatistic()
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            => _statistic.GetStatistic();
#else
            => default;
#endif

        public void Run(RunTaskHandler runTaskHandler)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            _loopProcessing.Run(runTaskHandler);
        }

        protected override void OnDispose()
        {
            if (_isInit)
            {
                _loopProcessing.Dispose();  //with waiting work threads.
                _worldData.Dispose();
                
                WorldGlobalRegister.Unregister((ushort)Id);
            }
        }

        private void InitInternal()
        {
            Id = WorldGlobalRegister.Register(this);

            TryApplyWorldData();
            _isInit = true;
        }

        private void TryDisposeWorldData()
        {
            TryDisposeState();
        }

        private void TryApplyWorldData()
        {
            AutoAttachSystems();
            FlattenSystems();

            TryApplyState();
            RequestTick = CurrentTick;
        }

        private void AutoAttachSystems()
        {
            WorldHelper.AutoAttachSystems(ref _worldData);
        }

        private void FlattenSystems()
        {
            _flatSystemsCache = WorldHelper.FlattenSystems(this);
        }

        private void TryDisposeState()
        {
            if (!_worldData.IsEmpty)
            {
                _loopProcessing.BreakAndWait();
                _loopProcessing.DetachToStateModule();
                _loopProcessing.Wait();

                _worldData.State.Dispose();
                _worldData.State = null;
            }
        }

        private void TryApplyState()
        {
            if (!_worldData.IsEmpty)
            {
                _loopProcessing.Wait();

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
                _statistic = new Debug.Diagnostic.WorldStatistic();
                _statistic.Construct(this);
                _statistic.UpdateSystemGraph(_worldData.Systems);
                _loopProcessing.SystemProcessing.SetStatistic(_statistic);
#endif
                _loopProcessing.Prepare(_worldData.State, _flatSystemsCache);
                _loopProcessing.AttachToStateModule();
            }
        }
    }
}