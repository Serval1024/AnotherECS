using AnotherECS.Core;
using System;

namespace AnotherECS.Converter
{
    public class RuntimeComponentConverter<EState> : RuntimeBindStateTypeToIdConverter<ushort, IComponent, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeComponentConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }
    }
}