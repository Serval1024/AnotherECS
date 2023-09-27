using AnotherECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Converter
{
    public class RuntimeStateConverter : IgnoresTypeToIdConverter<ushort, IState>, ITypeToUshort
    {
        public RuntimeStateConverter(Type[] ignoreTypes) 
            : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
          => base
                .GetSortTypes()
                .Where(p => p.BaseType == typeof(State));
    }
}