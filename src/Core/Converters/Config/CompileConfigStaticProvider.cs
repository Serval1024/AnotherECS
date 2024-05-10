using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Converter
{
    public static class CompileConfigIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = CompileConfigIdStaticProvider<EState>.Instance.TypeToId(typeof(TType));
    }

    public static class CompileConfigIdStaticProvider<EState>
       where EState : IState
    {
        private static CompileConfigToIdConverter<EState> _instance = null;
        private static readonly object _locker = new();

        public static CompileConfigToIdConverter<EState> Instance
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

    public class CompileConfigToIdConverter<EState> : CompileTypeToIdConverter<ushort, IComponent, EState>
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => GlobalRegister<EState, IConfig>.Data;
    }
}