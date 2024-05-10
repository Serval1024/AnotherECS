using AnotherECS.Core.Converter;
using System;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Core
{
    public class ReflectionSystemAutoAttachRegister : ISystemAutoAttachRegister
    {
        private readonly Type[] _cache;

        public ReflectionSystemAutoAttachRegister()
        {
            _cache = ReflectionSystemGlobalRegister.Instance.GetOrders()
                .Select(p => p.Key)
                .Where(p => p.GetCustomAttribute<ModuleAutoAttachAttribute>() != null)
                .ToArray();
        }

        public Type[] Gets()
            => _cache;
    }
}


