using AnotherECS.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Converter
{
    public class RuntimeSignalConverter<EState> : RuntimeBindStateTypeToIdConverter<ushort, ISignal, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeSignalConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .OrderBy(p => p.Name);
    }
}