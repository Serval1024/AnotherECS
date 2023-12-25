using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public class World<TState> : IWorld, IDisposable
        where TState : State, new()
    {
        private readonly IGroupSystemInternal _systems;
        private readonly TState _state;

        private readonly List<ITickEvent> _eventTemp = new();

        public World(IEnumerable<ISystem> systems)
            : this(new SystemGroup(systems), new TState()) { }

        public World(IEnumerable<ISystem> systems, TState state)
            : this(new SystemGroup(systems), state) { }

        public World(SystemGroup systems, TState state)
        {
            _systems = systems ?? throw new ArgumentNullException(nameof(systems));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void Init()
        {
            foreach(var system in SystemAutoAttachGlobalRegister.Gets())
            {
                _systems.Prepend((ISystem)Activator.CreateInstance(system));
            }

            _systems.PrepareInternal();
            _systems.ConstructInternal(_state);
            _state.FirstStartup();
            _systems.InitInternal(_state);
        }

        public void Tick()
            => Tick(1);

        public void Tick(uint tickCount)
        {
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
            => _systems.DestroyInternal(_state);

        public void OneTick()
        {
            _state.TickStarted();
            _systems.TickStartedInternal(_state);

            _eventTemp.Clear();
            _state.GetEvent(_eventTemp);
            _systems.ReceiveInternal(_state, _eventTemp);
            
            _systems.TickInternal(_state);

            _state.TickFinished();
            _systems.TickFinishiedInternal(_state);
        }

        public void Send(BaseEvent @event)
            => _state.Send(@event);

        public void Dispose()
        {
            _systems.Dispose();
            _state.Dispose();
        }
    }
}