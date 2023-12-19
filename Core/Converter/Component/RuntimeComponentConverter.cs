using System;
using System.Linq;
using System.Collections.Generic;
using AnotherECS.Core;

namespace AnotherECS.Converter
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