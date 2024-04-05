using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Serializer
{
    public class Dependencies
    {
        private readonly Dictionary<Type, Dictionary<uint, object>> _dependencies;

        public Dependencies(IEnumerable<DependencySerializer> dependencies)
        {
            _dependencies = new Dictionary<Type, Dictionary<uint, object>>();
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    Add(dependency.id, dependency.value);
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
        public T Get<T>(uint dependencyId)
          => (T)_dependencies[typeof(T)][dependencyId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>()
            => (T)_dependencies[typeof(T)][0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(Type type, uint dependencyId, object dependency)
        {
            if (_dependencies.TryGetValue(type, out var dict))
            {
                dict.Add(dependencyId, dependency);
            }
            else
            {
                _dependencies.Add(type, new Dictionary<uint, object>() { { dependencyId, dependency } });
            }
        }
    }
}
