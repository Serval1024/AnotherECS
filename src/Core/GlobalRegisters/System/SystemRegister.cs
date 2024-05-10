using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public class SystemRegister : ISystemRegister
    {
        private readonly Dictionary<Type, int> _orders = new();
        private readonly Dictionary<Type, InjectData> _injects = new();

        public void Install<T>(int order, string[] memberNameInjectAttributes = null)
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

        public Dictionary<Type, int> GetOrders()
        {
            lock (_orders)
            {
                return _orders;
            }
        }
        public Dictionary<Type, InjectData> GetInjects()
        {
            lock (_injects)
            {
                return _injects;
            }
        }
    }
}


