using System.Collections.Generic;
using AnotherECS.Core.Processing;

namespace AnotherECS.Core
{
    public static class WorldFactory
    {
        public static World Create<TState>(IEnumerable<ISystem> systems, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            where TState : State, new()
            => Create(systems, new TState(), threadingLevel);

        public static World Create(IEnumerable<ISystem> systems, State state, WorldThreadingLevel threadingLevel = WorldThreadingLevel.MainThreadOnly)
            => new(systems, state, SystemProcessingFactory.Create(state, threadingLevel));
    }

    public enum WorldThreadingLevel
    {
        MainThreadOnly = 0,
        OneThread = 1,
    }
}
