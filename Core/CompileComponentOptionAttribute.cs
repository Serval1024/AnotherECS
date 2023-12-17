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
        HistoryByVersion = 1 << 2,
        DataFree = 1 << 3,
        NotDataFree = 1 << 4,
        WithoutSparseDirectDense = 1 << 5,
        CompileFastAccess = 1 << 6,
        UseISerialize = 1 << 7,
    }
}
