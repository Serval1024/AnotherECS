using System;

namespace AnotherECS.Core.Threading
{
    internal static class ThreadUtils
    {
        public static int GetThreadCount()
            => Math.Clamp(GetProcessorCount() - 1, 1, int.MaxValue);

        public static int GetProcessorCount()
            => Environment.ProcessorCount;
    }
}