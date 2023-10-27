using AnotherECS.Core;

namespace AnotherECS.Converter
{
    public static class CompileComponentIdProvider<EState, TType>
       where EState : IState
    {
        public static ushort ID = StaticCompileComponentIdProvider<EState>.componentConverter.TypeToId(typeof(TType));
    }

    public static class StaticCompileComponentIdProvider<EState>
       where EState : IState
    {
        public static CompileComponentToIdConverter<EState> componentConverter = new();
    }

    public class CompileComponentToIdConverter<EState> : CompileTypeToIdConverter<ushort, IComponent, EState>
       where EState : IState
    { }
}