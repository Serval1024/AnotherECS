using System;

namespace AnotherECS.Core
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class ComponentOptionAttribute : Attribute
    {
        public ComponentOptions Options { get; private set; }
        public int Capacity { get; private set; }

        public ComponentOptionAttribute(ComponentOptions options)
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
        DataFree = 1 << 4,
        NotDataFree = 1 << 5,
        StorageLimit255 = 1 << 6,
        StorageLimit65535 = 1 << 7,
        WithoutSparseDirectDense = 1 << 8,
        NoCompileFastAccess = 1 << 9,
        CompileSortAtLast = 1 << 10,
        UseISerialize = 1 << 11,
    }
}
