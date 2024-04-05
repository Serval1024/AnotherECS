using AnotherECS.Converter;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public class RuntimeConfigConverter<EState> : RuntimeBindStateTypeToIdConverter<ushort, IConfig, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeConfigConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .OrderBy(p => p.Name);
    }
}