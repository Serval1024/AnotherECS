using AnotherECS.Converter;

namespace AnotherECS.Core.Converter
{
    public abstract class CompileTypeToIdConverter<UId, TType, EState> : TypeToIdConverter<UId>
       where UId : unmanaged
       where TType : class
       where EState : IState
    { }
}

