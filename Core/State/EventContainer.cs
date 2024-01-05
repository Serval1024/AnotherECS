using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public interface IEventInvoke
    {
        void Invoke(State context, BaseEvent @event);
    }

    public struct EventContainer<UState, UEvent> : IEventInvoke
        where UState : State
        where UEvent : BaseEvent
    {
        private readonly Action<UState, UEvent> _call;

        public EventContainer(IReceiverSystem system, Type interfaceType, Type eventType)
        {
            var method = interfaceType.GetMethod(nameof(IReceiverSystem<UState, UEvent>.Receive));
            _call = (Action<UState, UEvent>)method.CreateDelegate(typeof(Action<UState, UEvent>), system);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(State context, BaseEvent @event)
            => _call((UState)context, (UEvent)@event);
    }
}