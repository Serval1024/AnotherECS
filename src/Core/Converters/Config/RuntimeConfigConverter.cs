using AnotherECS.Converter;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class RuntimeConfigIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = RuntimeConfigIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public static class RuntimeConfigIdStaticProvider<EState>
       where EState : IState
    {
        private static RuntimeConfigConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static RuntimeConfigConverter<EState> Instance
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