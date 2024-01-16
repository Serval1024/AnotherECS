namespace AnotherECS.Core.Processing
{
    internal static class SystemProcessingFactory
    {
        public static ISystemProcessing Create(State state, WorldThreadingLevel threadingLevel)
            => (threadingLevel) switch
            {
                WorldThreadingLevel.MainThreadOnly  => new OneThreadProcessing<MainThreadScheduler>(state, MainThreadScheduler.Create()),
                WorldThreadingLevel.OneThread       => new OneThreadProcessing<OneNonBlockThreadScheduler>(state, OneNonBlockThreadScheduler.Create()),
                                                  _ => throw new System.NotImplementedException()
            };
    }
}
