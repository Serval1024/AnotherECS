using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Processing;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Debug.Diagnostic")]
namespace AnotherECS.Core
{
    public class World : BDisposable, IWorldExtend, IWorldLiveLoop, ISerialize, ISerializeConstructor
    {
        public string Name { get; set; }
        public uint Id { get; private set; }
        public uint CurrentTick => _state.Tick;
        public uint RequestTick { get; private set; }
        public LiveState LiveState { get; private set; }

        private readonly bool _isOneGateAutoAttach;
        private readonly List<SignalCallback> _signalBuffer = new();
        private readonly Dictionary<Type, List<ISignalReceiver>> _signalReceivers = new();
        private bool _isOneGateCallCreate = true;

        public State State
        { 
            get
            {
#if !ANOTHERECS_RELEASE
                ExceptionHelper.ThrowIfDisposed(this);
#endif
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    SetState(value);
                }
            }
        }


#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private IWorldStatistic _statistic;
#endif
        private IGroupSystemInternal _systems;
        private readonly LoopProcessing _loopProcessing;
        private State _state;
        private ISystem[] _flatSystemsCache;


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
            _systems = new SystemGroup(
                systems ?? throw new ArgumentNullException(nameof(systems))
            );

            _loopProcessing = new LoopProcessing(systemProcessing);
            _isOneGateAutoAttach = true;
            _state = state;
        }

        public World(ref ReaderContextSerializer reader)
        {
            _loopProcessing = new LoopProcessing(reader.Dependency.Resolve<ISystemProcessing>());
            _isOneGateAutoAttach = false;

            Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasConfig<T>()
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_state == null)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            return _state.IsHasConfig<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World AddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_state == null)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            _state.AddConfig(data);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World SetOrAddConfig<T>(T data)
            where T : IConfig
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_state == null)
            {
                throw new NullReferenceException($"Set state first.");
            }
#endif
            _state.SetOrAddConfig(data);
            return this;
        }

        public void Init()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (LiveState != LiveState.Raw)
            {
                throw new InvalidOperationException($"The world has already been initialized.");
            }
#endif
            InitInternal();
            
            LiveState = LiveState.Inited;
        }

        public void Startup()
        {
#if !ANOTHERECS_RELEASE
            if (LiveState != LiveState.Inited)
            {
                throw new InvalidOperationException($"The world has not {LiveState.Inited}.");
            }
            else if (LiveState == LiveState.Startup)
            {
                throw new InvalidOperationException($"The world has already been {LiveState.Startup}.");
            }
#endif
            StartupInternal();

            LiveState = LiveState.Startup;
        }

        public void Tick()
        {
            Tick(1u);
        }

        public void Tick(uint tickCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            if (_isOneGateCallCreate)
            {
                _isOneGateCallCreate = false;
                _loopProcessing.Create();
            }

#if !ANOTHERECS_HISTORY_DISABLE
            uint revertTick = _loopProcessing.TryRevertTo(RequestTick, _state.GetNextTickForEvent());
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
            _state.FlushSignalCache(_signalBuffer);
            for(int i = 0; i < _signalBuffer.Count; ++i)
            {
                if (_signalReceivers.TryGetValue(_signalBuffer[i].Signal.GetType(), out var receivers))
                {
                    for(int j = 0; j < receivers.Count; ++j)
                    {
                        switch (_signalBuffer[i].Command)
                        {
                            case SignalCallback.CommandType.Fire:
                                {
                                    receivers[j].OnFire(_signalBuffer[i].Signal);
                                    break;
                                }
                            case SignalCallback.CommandType.Cancel:
                                {
                                    receivers[j].OnCancel(_signalBuffer[i].Signal);
                                    break;
                                }
                            case SignalCallback.CommandType.LeaveBuffer:
                                {
                                    receivers[j].OnHistoryBufferLeave(_signalBuffer[i].Signal);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        public void RevertTo(uint tick)
        {
            _loopProcessing.RevertTo(tick);
        }

        public void Destroy()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
            if (LiveState == LiveState.Destroy)
            {
                throw new InvalidOperationException($"The world has already been {LiveState.Destroy}.");
            }
#endif
            _loopProcessing.Destroy();
        }

        public void UpdateFromMainThread()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            _loopProcessing.CallFromMainThread();
        }

        public void SendEvent(IEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            _state.Send(@event);
        }

        public void SendEvent(ITickEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            _state.Send(@event);
        }

        public void AddSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldRaw(this, LiveState);
#endif
            if (_signalReceivers.TryGetValue(typeof(TSignal), out List<ISignalReceiver> signalReceivers))
            {
                signalReceivers.Add(receiver);
            }
            else
            {
                _signalReceivers[typeof(TSignal)] = new() { receiver };
            }
        }

        public void RemoveSignal<TSignal>(ISignalReceiver<TSignal> receiver)
           where TSignal : ISignal
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldRaw(this, LiveState);
#endif
            if (_signalReceivers.TryGetValue(typeof(TSignal), out List<ISignalReceiver> signalReceivers))
            {
                signalReceivers.Remove(receiver);
            }
        }

        public bool IsBusy()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            return _loopProcessing.IsBusy();
        }

        public void Wait()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldInvalid(this, LiveState);
#endif
            _loopProcessing.Wait();
        }

        public TModuleData GetModuleData<TModuleData>(uint id)
            where TModuleData : IModuleData
            => _state.GetModuleData<TModuleData>(id);

        public void SetModuleData<TModuleData>(uint id, TModuleData data)
            where TModuleData : IModuleData
        {
            _state.SetModuleData(id, data);
        }

        public WorldStatisticData GetStatistic()
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            => _statistic.GetStatistic();
#else
            => default;
#endif

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_isOneGateCallCreate);
            writer.Pack(_systems);
            writer.Pack(Name);
            writer.Pack(_state);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _isOneGateCallCreate = reader.ReadBoolean();
            _systems = reader.Unpack<SystemGroup>();

            Name = reader.Unpack<string>();
            _state = reader.Unpack<State>();

            LiveState = LiveState.Raw;
        }

        public void Run(RunTaskHandler runTaskHandler)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldRaw(this, LiveState);
#endif
            _loopProcessing.Run(runTaskHandler);
        }

        protected override void OnDispose()
        {
            if (LiveState > LiveState.Raw)
            {
                LiveState = LiveState.Disposing;

                _loopProcessing.Dispose();  //with waiting work threads.

                _systems.Dispose();
                _state.Dispose();
                WorldGlobalRegister.Unregister((ushort)Id);

            }
            LiveState = LiveState.Disposed;
        }

        private void InitInternal()
        {
            Id = WorldGlobalRegister.Register(this);

            AutoAttachSystems();
            FlattenSystems();

            if (_state != null)
            {
                ApplyState();
                RequestTick = CurrentTick;
            }
        }

        private void AutoAttachSystems()
        {
            if (_isOneGateAutoAttach)
            {
                foreach (var system in SystemAutoAttachGlobalRegister.Gets())
                {
                    _systems.Prepend((ISystem)Activator.CreateInstance(system));
                }
            }
        }

        private void FlattenSystems()
        {
#if !ANOTHERECS_RELEASE
            var container = new WorldDIContainer(_state);
#else
            var container = new WorldDIContainer(_state, SystemGlobalRegister.GetInjects());
#endif
            _systems.Sort();
            var context = new InstallContext(this);
            container.Inject(_systems.GetSystemsAll());

            _systems.Install(ref context);
            if (context.IsAny())
            {
                _systems.Append(context.GetSystemGroup());
                _systems.Sort();
            }
            var systems = _systems.GetSystemsAll().ToArray();
            container.Inject(systems);

            _flatSystemsCache = systems;
        }

        private void StartupInternal()
        {
            _state.FirstStartup();
        }

        private void SetState(State state)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
#endif
            if (LiveState != LiveState.Inited && LiveState != LiveState.Startup)
            {
                throw new InvalidOperationException($"The world has not {LiveState.Inited}.");
            }

            if (_state != null)
            {
                _loopProcessing.BreakAndWait();
                _loopProcessing.DetachToStateModule();
                _loopProcessing.Wait();

                _state.Dispose();
                _state = null;
            }

            _state = state;

            if (_state != null)
            {
                ApplyState();
            }
        }

        private void ApplyState()
        {
            _loopProcessing.Prepare(_state, _flatSystemsCache);
            _loopProcessing.AttachToStateModule();

            if (LiveState == LiveState.Startup)
            {
                StartupInternal();
            }

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            _statistic = new Debug.Diagnostic.WorldStatistic();
            _statistic.Construct(this);
            _statistic.UpdateSystemGraph(_systems);
            _loopProcessing.SystemProcessing.SetStatistic(_statistic);
#endif
            _loopProcessing.Wait();
        }
    }
}