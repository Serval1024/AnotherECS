using AnotherECS.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Converter
{
    public class RuntimeComponentConverter<EState> : RuntimeBindStateTypeToIdConverter<ushort, IComponent, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeComponentConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .OrderBy(p => ComponentUtils.IsMarker(p) ? 1 : 0)
                .ThenBy(p => p.Name);
    }
}