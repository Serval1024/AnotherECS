using System.Collections.Generic;
using System.Linq;
using System;

namespace AnotherECS.Core.Threading
{
    public static class ReflectionUtils
    {
        public static Dictionary<Type, IEventInvoke> GetEventMap(IReceiverSystem receiverSystem)
        {
            var result = new Dictionary<Type, IEventInvoke>();

            foreach (var receiverSystemGeneric in receiverSystem
                .GetType()
                .GetInterfaces()
                .Where(p => p.Name.StartsWith($"{nameof(IReceiverSystem)}`")))
            {
                var args = receiverSystemGeneric.GetGenericArguments();
                if (args.Length == 2)
                {
                    Type eventType = args[1];
                    Type containerType = typeof(EventContainer<,>);

                    Type containerTypeGeneric = containerType.MakeGenericType(args);
                    var eventContainer = (IEventInvoke)Activator.CreateInstance(containerTypeGeneric, receiverSystem, receiverSystemGeneric, eventType);

                    result.Add(eventType, eventContainer);
                }
            }

            return result;
        }

    }
}