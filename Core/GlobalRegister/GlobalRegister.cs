using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class GlobalRegister<TState, TElementType>
        where TState : IState
    {
        private static readonly List<Type> _data = new();

        public static List<Type> Data => _data;

        public static void Install<T>()
        {
            lock (_data)
            {
                _data.Add(typeof(T));
            }
        }
    }
}