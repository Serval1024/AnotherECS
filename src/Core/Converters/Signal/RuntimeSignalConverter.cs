using AnotherECS.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Converter
{
    public static class RuntimeSignalIdProvider<EState, TType>
        where EState : IState
    {
        public static ushort ID = RuntimeSignalIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public static class RuntimeSignalIdStaticProvider<EState>
       where EState : IState
    {
        private static RuntimeSignalConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static RuntimeSignalConverter<EState> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        _instance ??= new(null);
                    }
                }
                return _instance;
            }
        }
    }

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