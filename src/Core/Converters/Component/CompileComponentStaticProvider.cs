using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class CompileComponentIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = CompileComponentIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public static class CompileComponentIdStaticProvider<EState>
       where EState : IState
    {
        private static CompileComponentToIdConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static CompileComponentToIdConverter<EState> Instance
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

    public class CompileComponentToIdConverter<EState> : CompileTypeToIdConverter<ushort, IComponent, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, IComponent>.Data;
    }
}