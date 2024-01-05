using System;

namespace AnotherECS.Core.Threading
{
    public static class ThreadUtils
    {
        public static int GetThreadCount()
            => Math.Clamp(Environment.ProcessorCount - 1, 1, int.MaxValue);
    }
}