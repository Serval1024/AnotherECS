using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class CompileComponentIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = CompileComponentIdStaticProvider<EState>.converter.TypeToId(typeof(TType));
    }

    public static class CompileComponentIdStaticProvider<EState>
       where EState : IState
    {
        public static CompileComponentToIdConverter<EState> converter = new();
    }

    public class CompileComponentToIdConverter<EState> : CompileTypeToIdConverter<ushort, IComponent, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, IComponent>.Data;
    }
}