using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public abstract class CompileTypeToIdConverter<UId, TType, EState> : TypeToIdConverter<UId>
       where UId : unmanaged
       where TType : class
       where EState : IState
    { }
}

