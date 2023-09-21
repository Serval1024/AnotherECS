using AnotherECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Activation;

namespace AnotherECS.Converter
{
    public class RuntimeTypeToIdConverter<UId, TType, EState> : IgnoresTypeToIdConverter<UId, TType>
        where UId : unmanaged
        where TType : class
        where EState : IState
    {
        public RuntimeTypeToIdConverter(Type[] ignoreTypes) 
            : base(ignoreTypes) { }

        protected override IEnumerable<Type> GetSortTypes()
            => base
                .GetSortTypes()
                .Where(p => !p.IsAbstract)
                .Where(p =>
                    {
                        var attribute = p.GetCustomAttribute<BindStateAttribute>();
                        return attribute == null || attribute.State == typeof(EState);                        
                    }
                    )
                .OrderBy(p => ComponentUtils.IsSortAtLast(p) ? 1 : 0)
                .ThenBy(p => p.Name);
    }
}

