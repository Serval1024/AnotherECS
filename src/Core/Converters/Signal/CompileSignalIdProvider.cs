using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class CompileSignalIdProvider<EState, TType>
        where EState : IState
    {
        public static ushort ID = CompileSignalIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public static class CompileSignalIdStaticProvider<EState>
       where EState : IState
    {
        private static CompileSignalToIdConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static CompileSignalToIdConverter<EState> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        _instance ??= new();
                    }
                }
                return _instance;
            }
        }
    }

    public class CompileSignalToIdConverter<EState> : CompileTypeToIdConverter<ushort, ISignal, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, ISignal>.Data;
    }
}