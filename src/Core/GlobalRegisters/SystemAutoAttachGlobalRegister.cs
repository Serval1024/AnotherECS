using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    public static class SystemAutoAttachGlobalRegister
    {
        private static readonly HashSet<Type> _data = new();

        public static void Install<T>()
            where T : ISystem
        {
            lock (_data)
            {
                if (!_data.Contains(typeof(T)))
                {
                    _data.Add(typeof(T));
                }
            }
        }

        public static Type[] Gets()
        {
            lock (_data)
            {
                return _data.ToArray();
            }
        }
    }
}


