using AnotherECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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
                        var types = ReflectionUtils.ExtractGenericFromInterface<TType>(p);
                        if (types.Length != 0)
                        {
                            return types.Any(p1 => p1 == typeof(EState));
                        }
                        return true;
                    }
                    )
                .OrderBy(p => ComponentUtils.IsSortAtLast(p) ? 1 : 0)
                .ThenBy(p => p.Name);
    }
}

