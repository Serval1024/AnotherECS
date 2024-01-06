using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class WorldFactory
    {
        public static World<TState> Create<TState>(IEnumerable<ISystem> systems, TState state, ThreadingLevel threadingLevel)
            where TState : State, new()
            => new(systems, state, SystemProcessingFactory.Create(state, threadingLevel));
    }

    public enum ThreadingLevel
    {
        MainThreadOnly = 0,
        NonBlockOneThread = 1,
        BlockMultiThread = 2,
        NonBlockMultiThread = 3,
    }
}
