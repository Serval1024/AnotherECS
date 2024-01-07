using AnotherECS.Core.Threading;

namespace AnotherECS.Core
{
    internal static class SystemProcessingFactory
    {
        public static ISystemProcessing Create(State state, ThreadingLevel threadingLevel)
            => (threadingLevel) switch
            {
                ThreadingLevel.MainThreadOnly       => new MainThreadProcessing(state),
                ThreadingLevel.NonBlockOneThread    => new OneThreadProcessing<OneNonBlockThreadScheduler>(state, OneNonBlockThreadScheduler.Create()),
                ThreadingLevel.BlockMultiThread     => new MultiThreadProcessing<BlockThreadScheduler>(state, BlockThreadScheduler.Create()),
                ThreadingLevel.NonBlockMultiThread  => new MultiThreadProcessing<NonBlockThreadScheduler>(state, NonBlockThreadScheduler.Create()),
                _ => throw new System.NotImplementedException()
            };
    }
}
