namespace AnotherECS.Core.Processing
{
    internal static class SystemProcessingFactory
    {
        public static ISystemProcessing Create(WorldThreadingLevel threadingLevel)
            => (threadingLevel) switch
            {
                WorldThreadingLevel.MainThreadOnly  => new OneThreadProcessing<MainThreadScheduler>(MainThreadScheduler.Create()),
                WorldThreadingLevel.OneThread       => new OneThreadProcessing<OneNonBlockThreadScheduler>(OneNonBlockThreadScheduler.Create()),
                                                  _ => throw new System.NotImplementedException()
            };
    }
}
