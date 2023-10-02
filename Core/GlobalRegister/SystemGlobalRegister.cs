using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class SystemGlobalRegister
    {
        private static readonly Dictionary<Type, int> _data = new();

        public static void Install<T>(int order)
            where T : ISystem
        {
            lock (_data)
            {
                if (!_data.ContainsKey(typeof(T)))
                {
                    _data.Add(typeof(T), order);
                }
            }
        }

        public static Dictionary<Type, int> GetOrders()
        {
            lock (_data)
            {
                return _data;
            }
        }
    }
}