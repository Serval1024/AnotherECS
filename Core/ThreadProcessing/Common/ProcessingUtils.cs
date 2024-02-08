using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Processing
{
    internal static class ProcessingUtils
    {
        public static Receivers ToReceivers(IEnumerable<IReceiverSystem> systems)
        {
            var receivers = Receivers.Create();

            foreach (var system in systems)
            {
                foreach (var element in Threading.ReflectionUtils.GetEventMap(system))
                {
                    receivers.Add(element.Key, element.Value);
                }
            }

            return receivers;
        }
    }

    internal struct Receivers
    {
        private Dictionary<Type, List<IEventInvoke>> _data;
        
        public static Receivers Create()
            => new()
            {
                _data = new Dictionary<Type, List<IEventInvoke>>()
            };

        public void Add(Type type, IEventInvoke @event)
        {
            if (_data.TryGetValue(type, out List<IEventInvoke> list))
            {
                list.Add(@event);
            }
            else
            {
                _data.Add(type, new List<IEventInvoke>() { @event });
            }
        }

        public void Receive(State state, List<ITickEvent> events)
        {
            for(int i = 0; i < events.Count; ++i)
            {
                Receive(state, events[i].Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Receive(State state, IEvent @event)
        {
            var value = @event.GetType();
            while (value != null)
            {
                if (_data.TryGetValue(value, out var systems))
                {
                    for (int j = 0; j < systems.Count; ++j)
                    {
                        systems[j].Invoke(state, @event);
                    }
                }
                value = value.BaseType;
            }
        }
    }
}
