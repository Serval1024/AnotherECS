using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class GlobalRegister<TState, TElementType>
        where TState : IState
    {
        private static readonly List<Type> _data = new();

        public static List<Type> Data => _data;

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ReloadDomainOptimizationHack()
        {
            _data.Clear();
        }
#endif

        public static void Install<T>()
        {
            lock (_data)
            {
                _data.Add(typeof(T));
            }
        }
    }
}