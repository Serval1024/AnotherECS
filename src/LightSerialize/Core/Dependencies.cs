using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Serializer
{
    public struct Dependencies
    {
        private readonly Dictionary<Type, Dictionary<uint, object>> _dependencyByType;
        private readonly List<object> _dependencyList;

        public Dependencies(IEnumerable<DependencySerializer> dependencies)
        {
            _dependencyByType = new Dictionary<Type, Dictionary<uint, object>>();
            _dependencyList = new List<object>();
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    AddInternal(dependency.value.GetType(), dependency.id, dependency.value);
                }
            }
        }

        public void Add<T>(T dependency)
        {
            AddInternal(typeof(T), 0, dependency);
        }

        public void Add<T>(uint dependencyId, T dependency)
        {
            AddInternal(typeof(T), dependencyId, dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DirectGet<T>(uint dependencyId)
          => (T)_dependencyByType[typeof(T)][dependencyId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DirectGet<T>()
            => DirectGet<T>(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Resolve<T>()
        {
            foreach(var dependency in _dependencyList)
            {
                if (typeof(T).IsAssignableFrom(dependency.GetType()))
                {
                    return (T)dependency;
                }
            }
            throw new ArgumentException("Dependency not found.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(Type type, uint dependencyId, object dependency)
        {
            if (_dependencyByType.TryGetValue(type, out var dict))
            {
                dict.Add(dependencyId, dependency);
            }
            else
            {
                _dependencyByType.Add(type, new Dictionary<uint, object>() { { dependencyId, dependency } });
            }
            _dependencyList.Add(dependency);
        }
    }
}
