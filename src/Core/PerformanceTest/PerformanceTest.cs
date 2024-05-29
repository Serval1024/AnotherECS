namespace AnotherECS.Core
{
    public static class PerformanceTester
    {
        private static PerformanceTesterImpl _impl = new();        

        public static long Do()
            => _impl.Do();
    }

    public class PerformanceTesterImpl
    {
        private const int ITERATION = 10000;

        internal uint count = 0;
        internal long lastResult = -1;

        public long Do()
        {
            if (lastResult == -1)
            {
                lastResult = DoInternal();
            }
            return lastResult;
        }

        public void DropCache()
        {
            lastResult = -1;
        }

        private long DoInternal()
        {
            count = 0;

            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < ITERATION; i++)
            {
                ++count;
            }
            timer.Stop();
            return timer.ElapsedTicks;
        }
    }
}
