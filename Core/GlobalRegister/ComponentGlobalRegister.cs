using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class ComponentGlobalRegister<U>
        where U : IState
    {
        private static readonly List<Type> _data = new();

        public static List<Type> Data => _data;

        public static void Install<T>()
            where T : IComponent
        {
            lock (_data)
            {
                _data.Add(typeof(T));
            }
        }
    }
}