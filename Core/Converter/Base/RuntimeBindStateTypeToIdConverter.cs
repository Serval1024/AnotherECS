using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public class RuntimeBindStateTypeToIdConverter<UId, TType, EState> : IgnoresTypeToIdConverter<UId, TType>
        where UId : unmanaged
        where TType : class
        where EState : IState
    {
        public RuntimeBindStateTypeToIdConverter(Type[] ignoreTypes) 
            : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .Where(p => !p.IsAbstract)
                .Where(p =>
                    {
                        var bindAttribute = p.GetCustomAttribute<BindStateAttribute>();
                        return bindAttribute == null || bindAttribute.State == typeof(EState);
                    }
                    );
    }
}

