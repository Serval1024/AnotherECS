using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Converter
{
    public abstract class IgnoresTypeToIdConverter<UId, TType> : TypeToIdConverter<UId, TType>
        where UId : unmanaged
        where TType : class
    {
        private readonly Type[] _ignoreTypes;

        public IgnoresTypeToIdConverter(Type[] ignoreTypes)
        {
            _ignoreTypes = ignoreTypes ?? Array.Empty<Type>();
            Init();
        }

        protected override void OnInit() { }

        protected override IEnumerable<Type> GetSortTypes()
           => base
                .GetSortTypes()
                .Where(p => !_ignoreTypes.Any(p0 => p0 == p));
    }
}

