using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering.VirtualTexturing;

namespace AnotherECS.Core
{
    public class SystemGroup :
        IConstructModule,
        ITickStartModule,
        ITickFinishiedModule,
        IInitSystem,
        ITickSystem,
        IDestroySystem,
        IReceiverSystem<BaseEvent>,
        IEnumerable<ISystem>,
        IDisposable,
        IGroupSystemInternal
    {
        private readonly List<ISystem> _systems = new();
        private readonly List<ITickSystem> _tickSystems = new();
        private readonly Dictionary<Type, List<ISystemContainer>> _receiverSystems = new();

        private readonly SortOrder _order;
        private bool _isDisposed;


        public SystemGroup(SortOrder order = SortOrder.Attributes)
        {
            _order = order;
        }

        public SystemGroup(IEnumerable<ISystem> systems, SortOrder order = SortOrder.Attributes)
            :this(order)
        {
            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            foreach (var system in systems)
            {
                Add(system);
            }
        }

        public SystemGroup Add(ISystem system)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfDisposed();
#endif
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            _systems.Add(system);
            return this;
        }

        public void Remove(ISystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            _systems.Remove(system);

            if (system is ITickSystem tickSystem)
            {
                _tickSystems.Remove(tickSystem);
            }
        }

        public IEnumerator<ISystem> GetEnumerator()
            => _systems.GetEnumerator();

        public void Construct(State state)
        {
            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] is IConstructModule prepareModuleSystem)
                {
                    prepareModuleSystem.Construct(state);
                }
            }
        }

        public void TickStarted(State state)
        {
            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] is ITickStartModule tickStartModule)
                {
                    tickStartModule.TickStarted(state);
                }
            }
        }

        public void TickFinishied(State state)
        {
            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] is ITickFinishiedModule tickFinishiedModule)
                {
                    tickFinishiedModule.TickFinishied(state);
                }
            }
        }

        public void Init(State state)
        {
            for (int i = 0; i < _systems.Count; ++i)
            {
                if (_systems[i] is IInitSystem initSystem)
                {
                    initSystem.Init(state);
                }
            }
        }
        public void Tick(State state)
        {
            for (int i = 0, iMax = _tickSystems.Count; i < iMax; ++i)
            {
                _tickSystems[i].Tick(state);
            }
        }

        public void Destroy(State state)
        {
            for (int i = 0, iMax = _tickSystems.Count; i < iMax; ++i)
            {
                if (_systems is IDestroySystem destroySystem)
                {
                    destroySystem.Destroy(state);
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                for (int i = 0; i < _systems.Count; ++i)
                {
                    if (_systems[i] is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _systems.Clear();
                _tickSystems.Clear();
            }
        }
     
        internal IEnumerable<ISystem> GetSystems()
            => _systems;

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Receive(State state, BaseEvent @event)
        {
            var value = @event.GetType();
            while (value != null)
            {
                if (_receiverSystems.TryGetValue(value, out var systems))
                {
                    for (int j = 0; j < systems.Count; ++j)
                    {
                        systems[j].Call(state, @event);
                    }
                }
                value = value.BaseType;
            }
        }

        void IGroupSystemInternal.ReceiveInternal(State state, List<ITickEvent> events)
        {
            for (int i = 0; i < events.Count; ++i)
            {
                Receive(state, events[i].Value);
            }
        }

        void IGroupSystemInternal.PrepareInternal()
        {
            _tickSystems.Clear();
            _receiverSystems.Clear();

            if (_order == SortOrder.Attributes)
            {
                var order = SystemGlobalRegister.GetOrders();
                _systems.Sort((p0, p1) =>
                     (order.TryGetValue(p0.GetType(), out int v0) && order.TryGetValue(p1.GetType(), out int v1)) 
                     ? (v0 - v1)
                     : 0
                );
            }

            foreach (var system in _systems)
            {
                if (system is ITickSystem tickSystem)
                {
                    _tickSystems.Add(tickSystem);
                }

                if (system is IReceiverSystem receiverSystem)
                {
                    foreach (var receiverSystemGeneric in receiverSystem
                        .GetType()
                        .GetInterfaces()
                        .Where(p => p.Name == $"{nameof(IReceiverSystem)}`2"))
                    {
                        var args = receiverSystemGeneric.GetGenericArguments();
                        if (args.Length == 2)
                        {
                            Type eventType = args[1];
                            Type containerType = typeof(EventContainer<,>);

                            Type containerTypeGeneric = containerType.MakeGenericType(args);
                            var eventContainer = (ISystemContainer)Activator.CreateInstance(containerTypeGeneric, receiverSystem, receiverSystemGeneric, eventType);

                            if (_receiverSystems.TryGetValue(eventType, out List<ISystemContainer> receivers))
                            {
                                receivers.Add(eventContainer);
                            }
                            else
                            {
                                _receiverSystems.Add(eventType, new List<ISystemContainer>() { eventContainer });
                            }
                        }
                    }
                }
            }

            foreach (var system in _systems)
            {
                if (system is IGroupSystemInternal initSystemInternal)
                {
                    initSystemInternal.PrepareInternal();
                }
            }
        }

        void IGroupSystemInternal.Prepend(ISystem system)
        {
            _systems.Insert(0, system);
        }

        void IGroupSystemInternal.ConstructInternal(State state)
        {
            Construct(state);
        }

        void IGroupSystemInternal.TickStartedInternal(State state)
        {
            TickStarted(state);
        }

        void IGroupSystemInternal.TickFinishiedInternal(State state)
        {
            TickFinishied(state);
        }

        void IGroupSystemInternal.InitInternal(State state)
        {
            Init(state);
        }

        void IGroupSystemInternal.TickInternal(State state)
        {
            Tick(state);
        }

        void IGroupSystemInternal.DestroyInternal(State state)
        {
            Destroy(state);
        }

#if !ANOTHERECS_RELEASE
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SystemGroup));
            }
        }
#endif

        private interface ISystemContainer
        {
            void Call(State context, BaseEvent @event);
        }

        private struct EventContainer<UState, UEvent> : ISystemContainer
            where UState : State
            where UEvent : BaseEvent
        {
            private readonly Action<UState, UEvent> _call;

            public EventContainer(IReceiverSystem system, Type interfaceType, Type eventType)
            {
                var method = interfaceType.GetMethod(nameof(IReceiverSystem<UEvent>.Receive));
                _call = (Action<UState, UEvent>)method.CreateDelegate(typeof(Action<UState, UEvent>), system);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Call(State context, BaseEvent @event)
                => _call((UState)context, (UEvent)@event);
        }

        public enum SortOrder
        {
            Attributes,
            Declaration,
        }
    }
}