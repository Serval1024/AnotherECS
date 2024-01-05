using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public class World<TState> : IWorld, IDisposable
        where TState : State, new()
    {
#if !ANOTHERECS_RELEASE
        private bool _isInit = false;
        private bool _isDispose = false;
#endif
        private readonly IGroupSystemInternal _systems;
        private readonly TState _state;
        private readonly LoopProcessing _loopProcessing;

        public World(IEnumerable<ISystem> systems)
            : this(systems, new TState()) { }

        public World(IEnumerable<ISystem> systems, TState state, ISystemProcessing systemProcessing = default)
        {
            _systems = new SystemGroup(
                systems ?? throw new ArgumentNullException(nameof(systems))
                );

            _state = state ?? throw new ArgumentNullException(nameof(state));

            _loopProcessing = new LoopProcessing(
                systemProcessing ?? SystemProcessingFactory.Create(state, ThreadingLevel.MainThreadOnly)
                );
        }

        public void Init()
        {
            foreach(var system in SystemAutoAttachGlobalRegister.Gets())
            {
                _systems.Prepend((ISystem)Activator.CreateInstance(system));
            }
            _systems.Sort();

            _state.FirstStartup();
            _loopProcessing.Init(_systems);
#if !ANOTHERECS_RELEASE
            _isInit = true;
#endif
        }

        public void Tick()
        {
            Tick(1);
        }

        public void Tick(uint tickCount)
        {
#if !ANOTHERECS_RELEASE
            if (!_isInit || _isDispose)
            {
                throw new InvalidOperationException("World not init yet or disposed.");
            }
#endif
            if (tickCount != 0)
            {
#if ANOTHERECS_HISTORY_DISABLE
                for (int i = 0; i < tickCount; ++i)
                {
                    OneTick();
                }
#else
                var targetTick = _state.Tick + tickCount;

                if (_state.GetNextTickForEvent() < targetTick)
                {
                    _state.RevertTo(_state.GetNextTickForEvent() - 1);
                }

                while (_state.Tick < targetTick)
                {
                    OneTick();
                }
#endif
            }
        }

        public void Destroy()
            => _loopProcessing.Destroy();

        public void UpdateFromMainThread()
        {
            _loopProcessing.CallFromMainThread();
        }

        public void OneTick()
        {
            _state.TickStarted();
            _loopProcessing.Tick();
        }


        public void Send(BaseEvent @event)
            => _state.Send(@event);

        public void Dispose()
        {
            _loopProcessing.Dispose();

            _systems.Dispose();
            _state.Dispose();
#if !ANOTHERECS_RELEASE
            _isDispose = true;
#endif
        }
    }
}