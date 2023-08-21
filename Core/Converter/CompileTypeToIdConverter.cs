using AnotherECS.Core;
using System;
using System.Collections.Generic;

namespace AnotherECS.Converter
{
    public class CompileTypeToIdConverter<UId, TType, EState> : TypeToIdConverter<UId, TType>
       where UId : unmanaged
       where TType : class
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => ComponentGlobalRegister<EState>.Data;
    }
}

