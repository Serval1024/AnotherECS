using System;
using System.Collections.Generic;
using System.Linq;
using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public class RuntimeSystemConverter : IgnoresTypeToIdConverter<ushort, ISystem>, ITypeToUshort
    {
        public RuntimeSystemConverter(Type[] ignoreTypes) 
            : base(ignoreTypes)
        {
        }

        protected override IEnumerable<Type> GetSortTypes()
            => SystemUtils.GetOrder(
                base.GetSortTypes()
                .Where(p => !p.IsAbstract)
                .ToArray()
                );
    }
}
