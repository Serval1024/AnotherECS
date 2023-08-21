using AnotherECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Converter
{
    public class RuntimeOrderSystem : IgnoresTypeToIdConverter<ushort, ISystem>, ITypeToUshort
    {
        public RuntimeOrderSystem(Type[] ignoreTypes) 
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
