using AnotherECS.Core.Threading;

namespace AnotherECS.Core
{
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
}
