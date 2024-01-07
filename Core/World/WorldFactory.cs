using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class WorldFactory
    {
        public static World Create<TState>(IEnumerable<ISystem> systems, ThreadingLevel threadingLevel = ThreadingLevel.MainThreadOnly)
            where TState : State, new()
            => Create(systems, new TState(), threadingLevel);

        public static World Create(IEnumerable<ISystem> systems, State state, ThreadingLevel threadingLevel = ThreadingLevel.MainThreadOnly)
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
