using AnotherECS.Core.Threading;

namespace AnotherECS.Core
{
    internal static class SystemProcessingFactory
    {
        public static ISystemProcessing Create(State state, WorldThreadingLevel threadingLevel)
            => (threadingLevel) switch
            {
                WorldThreadingLevel.MainThreadOnly       => new MainThreadProcessing(state),
                WorldThreadingLevel.NonBlockOneThread    => new OneThreadProcessing<OneNonBlockThreadScheduler>(state, OneNonBlockThreadScheduler.Create()),
                WorldThreadingLevel.BlockMultiThread     => new MultiThreadProcessing<BlockThreadScheduler>(state, BlockThreadScheduler.Create()),
                WorldThreadingLevel.NonBlockMultiThread  => new MultiThreadProcessing<NonBlockThreadScheduler>(state, NonBlockThreadScheduler.Create()),

                                                       _ => throw new System.NotImplementedException()
            };
    }
}
