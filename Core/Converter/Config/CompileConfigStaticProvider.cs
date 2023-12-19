using System;
using System.Collections.Generic;
using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public static class CompileConfigIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = CompileConfigIdStaticProvider<EState>.converter.TypeToId(typeof(TType));
    }

    public static class CompileConfigIdStaticProvider<EState>
       where EState : IState
    {
        public static CompileConfigToIdConverter<EState> converter = new();
    }

    public class CompileConfigToIdConverter<EState> : CompileTypeToIdConverter<ushort, IComponent, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, IConfig>.Data;
    }
}