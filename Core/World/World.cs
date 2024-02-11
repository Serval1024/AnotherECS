using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Debug.Diagnostic")]
namespace AnotherECS.Core
{


    public class World : BDisposable, IWorld
    {
        public string Name { get; set; }
        public uint CurrentTick => _state.Tick;
        public uint RequestTick { get; private set; }

#if !ANOTHERECS_RELEASE
        private bool _isInit = false;
#endif
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private readonly IWorldStatistic _statistic;
#endif
        private readonly IGroupSystemInternal _systems;
        private readonly State _state;
        private readonly LoopProcessing _loopProcessing;


        public World(ISystem system, State state, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(new SystemGroup(system), state, SystemProcessingFactory.Create(state, threadingLevel)) { }

        public World(IGroupSystem systems, State state, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            : this(systems, state, SystemProcessingFactory.Create(state, threadingLevel)) { }

        public World(IEnumerable<ISystem> systems, State state, ISystemProcessing systemProcessing = default)
        {
            if (systemProcessing == null)
            {
                throw new ArgumentNullException(nameof(systemProcessing));
            }

            _systems = new SystemGroup(
                systems ?? throw new ArgumentNullException(nameof(systems))
                );

            _state = state ?? throw new ArgumentNullException(nameof(state));

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            _statistic = new Debug.Diagnostic.WorldStatistic();
            _statistic.Construct(this);
            systemProcessing.SetStatistic(_statistic);
#endif
            _loopProcessing = new LoopProcessing(systemProcessing);
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
            _isInit = true;
#endif
            foreach (var system in SystemAutoAttachGlobalRegister.Gets())
            {
                _systems.Prepend((ISystem)Activator.CreateInstance(system));
            }
#if !ANOTHERECS_RELEASE
            var container = new WorldDIContainer(_state, SystemGlobalRegister.GetInjects());
#else
            var container = new WorldDIContainer(_state);
#endif
            _systems.Sort();
            var context = new InstallContext(this);
            container.Inject(_systems.GetSystemsAll());

            _systems.Install(ref context);
            _systems.Append(context.GetSystemGroup());
            _systems.Sort();
            container.Inject(_systems.GetSystemsAll());

            _state.FirstStartup();

            RequestTick = CurrentTick;
            _loopProcessing.Prepare(_state, _systems.GetSystemsAll());
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

        public void Send(IEvent @event)
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

        public WorldStatisticData GetStatistic()
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            => _statistic.GetStatistic();
#else
            => default;
#endif

        internal State GetState()
            => _state;

        protected override void OnDispose()
        {
            _loopProcessing.Dispose();  //with waiting work threads.

            _systems.Dispose();
            _state.Dispose();
        }
    }
}