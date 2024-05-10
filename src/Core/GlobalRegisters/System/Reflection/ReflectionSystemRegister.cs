using AnotherECS.Core.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    public class ReflectionSystemRegister : ISystemRegister
    {
        private readonly Dictionary<Type, int> _cache;

        public ReflectionSystemRegister()
        {
            _cache = new RuntimeSystemConverter(null)
                .GetAssociationTable()
                .ToDictionary(k => k.Value, v => (int)v.Key);
        }

        public Dictionary<Type, int> GetOrders()
            => _cache;
    }
}


