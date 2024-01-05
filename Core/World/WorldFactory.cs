using AnotherECS.Core.Threading;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public static class WorldFactory
    {
        public static World<TState> Create<TState>(IEnumerable<ISystem> systems, TState state, ThreadingLevel threadingLevel)
            where TState : State, new()
            => new(systems, state, SystemProcessingFactory.Create(state, threadingLevel));
    }

    internal static class SystemProcessingFactory
    {
        public static ISystemProcessing Create(State state, ThreadingLevel threadingLevel)
            => (threadingLevel) switch
            {
                ThreadingLevel.MainThreadOnly       => new MainThreadProcessing(state),
                ThreadingLevel.NonBlockOneThread    => new OneThreadProcessing(state, new NonBlockThreadScheduler()),
                ThreadingLevel.BlockMultiThread     => new MultiThreadProcessing(state, new BlockThreadScheduler()),
                ThreadingLevel.NonBlockMultiThread  => new MultiThreadProcessing(state, new NonBlockThreadScheduler()),
                _ => throw new System.NotImplementedException()
            };
    }

    public enum ThreadingLevel
    {
        MainThreadOnly = 0,
        NonBlockOneThread = 1,
        BlockMultiThread = 2,
        NonBlockMultiThread = 3,
    }
}
