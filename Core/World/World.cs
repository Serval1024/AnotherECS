using System;
using System.Collections.Generic;
using AnotherECS.Core.Processing;

namespace AnotherECS.Core
{
    public class World : BDisposable, IWorld
    {
        public uint CurrentTick => _state.Tick;
        public uint RequestTick { get; private set; }

#if !ANOTHERECS_RELEASE
        private bool _isInit = false;
#endif
        private readonly IGroupSystemInternal _systems;
        private readonly State _state;
        private readonly LoopProcessing _loopProcessing;

        public World(IEnumerable<ISystem> systems, State state, ISystemProcessing systemProcessing = default)
        {
            _systems = new SystemGroup(
                systems ?? throw new ArgumentNullException(nameof(systems))
                );

            _state = state ?? throw new ArgumentNullException(nameof(state));

            _loopProcessing = new LoopProcessing(
                systemProcessing ?? SystemProcessingFactory.Create(state, WorldThreadingLevel.MainThreadOnly)
                );
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
            _systems.Sort();

            _state.FirstStartup();

            RequestTick = CurrentTick;
            _loopProcessing.Prepare(_state, _systems);
            _loopProcessing.Init();
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

        public void RevertTo(uint tickCount)
        {
            _loopProcessing.RevertTo(tickCount);
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

        public void Send(BaseEvent @event)
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

        protected override void OnDispose()
        {
            _loopProcessing.Dispose();  //with waiting work threads.

            _systems.Dispose();
            _state.Dispose();
        }
    }
}