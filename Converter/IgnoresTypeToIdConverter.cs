using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Converter
{
    public class IgnoresTypeToIdConverter<UId, TType> : TypeToIdConverter<UId>
        where UId : unmanaged
        where TType : class
    {
        private readonly Type[] _ignoreTypes;

        public IgnoresTypeToIdConverter(Type[] ignoreTypes = null)
        {
            _ignoreTypes = ignoreTypes ?? Array.Empty<Type>();
            Init();
        }

        protected override void OnInit() { }

        protected override IEnumerable<Type> GetSortTypes()
           => TypeUtils.GetRuntimeTypes<TType>()
                .OrderBy(p => p.Name)
                .Where(p => p.GetCustomAttribute<IgnoreCompileAttribute>() == null)
                .Where(p => !_ignoreTypes.Any(p0 => p0 == p));
    }
}

