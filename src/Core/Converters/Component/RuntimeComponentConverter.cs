using AnotherECS.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Converter
{
    public static class RuntimeComponentIdProvider<EState, TType>
      where EState : IState
    {
        public static ushort ID = RuntimeComponentIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public class RuntimeComponentConverter<EState> : RuntimeBindStateTypeToIdConverter<ushort, IComponent, EState>, ITypeToUshort
        where EState : IState
    {
        public RuntimeComponentConverter(Type[] ignoreTypes)
           : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .OrderBy(p => ComponentUtils.IsMarker(p) ? 1 : 0)
                .ThenBy(p => p.Name);
    }

    public static class RuntimeComponentIdStaticProvider<EState>
        where EState : IState
    {
        private static RuntimeComponentConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static RuntimeComponentConverter<EState> Instance
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
}