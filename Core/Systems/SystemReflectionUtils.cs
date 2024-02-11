using AnotherECS.Core.Inject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Core
{
    internal static class SystemReflectionUtils
    {
        public const BindingFlags MEMBER_INJECT_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

        public static IEnumerable<MemberInfo> GetMemberInjectAttributes(Type type)
            => type
                .GetFieldsAndProperties(MEMBER_INJECT_FLAGS)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null);
    }
}