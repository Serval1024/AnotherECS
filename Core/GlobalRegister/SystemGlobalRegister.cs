using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class SystemGlobalRegister
    {
        private static readonly Dictionary<Type, int> _orders = new();
        private static readonly Dictionary<Type, InjectData> _injects = new();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _injects.Clear();
        }
#endif

        public static void Install<T>(int order, string[] memberNameInjectAttributes = null)
            where T : ISystem
        {
            lock (_orders)
            {
                if (!_orders.ContainsKey(typeof(T)))
                {
                    _orders.Add(typeof(T), order);
                    if (memberNameInjectAttributes != null)
                    {
                        lock (_injects)
                        {
                            _injects.Add(typeof(T), new() { memberNameInjectAttributes = memberNameInjectAttributes });
                        }
                    }
                }
            }
        }

        public static Dictionary<Type, int> GetOrders()
        {
            lock (_orders)
            {
                return _orders;
            }
        }
        public static Dictionary<Type, InjectData> GetInjects()
        {
            lock (_injects)
            {
                return _injects;
            }
        }
    }
}


