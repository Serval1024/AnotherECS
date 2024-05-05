using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class CompileSignalIdProvider<EState, TType>
        where EState : IState
    {
        public static ushort ID = CompileSignalIdStaticProvider<EState>.converter.TypeToId(typeof(TType));
    }

    public static class CompileSignalIdStaticProvider<EState>
       where EState : IState
    {
        public static CompileSignalToIdConverter<EState> converter = new();
    }

    public class CompileSignalToIdConverter<EState> : CompileTypeToIdConverter<ushort, ISignal, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, ISignal>.Data;
    }
}