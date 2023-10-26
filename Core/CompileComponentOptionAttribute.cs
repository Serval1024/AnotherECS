using System;

namespace AnotherECS.Core
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class CompileComponentOptionAttribute : Attribute
    {
        public ComponentOptions Options { get; private set; }
        public int Capacity { get; private set; }

        public CompileComponentOptionAttribute(ComponentOptions options)
        {
            Options = options;
        }
    }

    [Flags]
    public enum ComponentOptions
    {
        HistoryNonSync = 1 << 1,
        HistoryByChange = 1 << 2,
        HistoryByTick = 1 << 3,
        HistoryByVersion = 1 << 4,
        DataFree = 1 << 5,
        NotDataFree = 1 << 6,
        WithoutSparseDirectDense = 1 << 9,
        CompileFastAccess = 1 << 10,
        UseISerialize = 1 << 112,
    }
}
