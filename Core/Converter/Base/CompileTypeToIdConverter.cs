using System;
using System.Collections.Generic;
using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public class CompileTypeToIdConverter<UId, TType, EState> : TypeToIdConverter<UId>
       where UId : unmanaged
       where TType : class
       where EState : IState
    {
        protected override IEnumerable<Type> GetSortTypes()
            => ComponentGlobalRegister<EState>.Data;
    }
}

