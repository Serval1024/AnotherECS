using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Debug.Diagnostic")]
namespace AnotherECS.Core
{
    public class World : BDisposable, IWorldExtend
    {
        public string Name { get; set; }
        public uint Id { get; }
        public uint CurrentTick => _state.Tick;
        public uint RequestTick { get; private set; }

#if !ANOTHERECS_RELEASE
        private bool _isInit = false;
#endif
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private IWorldStatistic _statistic;
#endif
        private readonly IGroupSystemInternal _systems;
        private readonly LoopProcessing _loopProcessing;
        private State _state;

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
            SetState(state);
            Id = WorldGlobalRegister.Register(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasConfig<T>()
            where T : IConfig
            => _state.IsHasConfig<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World AddConfig<T>(T data)
            where T : IConfig
        {
            _state.AddConfig(data);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public World SetOrAddConfig<T>(T data)
            where T : IConfig
        {
            _state.SetOrAddConfig(data);
            return this;
        }

        public void Init()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfDisposed(this);
            if (_isInit)
            {
                throw new InvalidOperationException($"The world has already been initialized.");
            }

            _isInit = true;
            if (_state == null)
            {
                throw new InvalidOperationException($"State is null.");
            }
#endif
            foreach (var system in SystemAutoAttachGlobalRegister.Gets())
            {
                _systems.Prepend((ISystem)Activator.CreateInstance(system));
            }
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

            _state.FirstStartup();

            RequestTick = CurrentTick;
            _loopProcessing.Prepare(_state, systems);
            _loopProcessing.Init();
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            _statistic.UpdateSystemGraph(_systems);
#endif
        }

        public void Tick()
        {
            Tick(1u);
        }

        public void Tick(uint tickCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            if (tickCount != 0)
            {
#if !ANOTHERECS_HISTORY_DISABLE
                _loopProcessing.TryRevertTo(RequestTick, _state.GetNextTickForEvent());
#endif
                RequestTick += tickCount;

                for (int i = 0; i < tickCount; ++i)
                {
                    _loopProcessing.Tick();   
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
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            _loopProcessing.Destroy();
        }

        public void UpdateFromMainThread()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            _loopProcessing.CallFromMainThread();
        }

        public void SendEvent(IEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            _state.Send(@event);
        }

        public void SendEvent(ITickEvent @event)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            _state.Send(@event);
        }

        public bool IsBusy()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            return _loopProcessing.IsBusy();
        }

        public void Wait()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfWorldDisposed(this, _isInit);
#endif
            _loopProcessing.Wait();
        }

        public State GetState()
            => _state;

        public void SetState(State state)
        {
            _loopProcessing.BreakAndWait();

            if (_state != null)
            {
                _state.Dispose();
                _state = null;
            }

            _state = state;
            _loopProcessing.SystemProcessing.Bind(_state);

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            _statistic = new Debug.Diagnostic.WorldStatistic();
            _statistic.Construct(this);
            _loopProcessing.SystemProcessing.SetStatistic(_statistic);
#endif
        }

        public WorldStatisticData GetStatistic()
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            => _statistic.GetStatistic();
#else
            => default;
#endif

        public ITickEvent ToITickEvent(IEvent @event)
            => _state.ToITickEvent(@event);


        protected override void OnDispose()
        {
            _loopProcessing.Dispose();  //with waiting work threads.

            _systems.Dispose();
            _state.Dispose();
            WorldGlobalRegister.Unregister((ushort)Id);
        }
    }
}