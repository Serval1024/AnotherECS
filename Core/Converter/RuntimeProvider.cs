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

    public class RuntimeComponentToIntConverter<EState> : RuntimeTypeToIdConverter<ushort, IComponent, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeComponentToIntConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }
    }

    public class RuntimeFilterToIntConverter<EState> : RuntimeTypeToIdConverter<ushort, ICompileFilter, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeFilterToIntConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .Where(p => !p.IsGenericType || !p.ContainsGenericParameters);    
    }
}