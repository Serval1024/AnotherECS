using AnotherECS.Unsafe;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AnotherECS.Core
{
    public unsafe static class FilterGlobalRegister<U>
        where U : IState
    {
        private static int _counter;
        private static readonly Dictionary<Type, Mask> _maskByType = new();

        public static void Install<T>(Func<State, int, bool> selector, bool isAutoClear, ushort[] include, ushort[] exclude)
            where T : IFilter
        {
            include ??= Array.Empty<ushort>();
            exclude ??= Array.Empty<ushort>();

            var id = (uint)Interlocked.Increment(ref _counter);
            
            var mask = new Mask(UnsafeUtils.ConvertToPointer(selector), id, isAutoClear, include, exclude);
            lock (_maskByType)
            {
                _maskByType.Add(typeof(T), mask);
            }
        }

        public static Mask GetMask(Type filter)
        {
            lock (_maskByType)
            {
                return _maskByType.TryGetValue(filter, out var mask) ? mask : default;
            }
        }
    }

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