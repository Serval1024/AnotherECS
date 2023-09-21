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
        HistoryNonSync = 1 << 0,
        HistoryByChange = 1 << 1,
        HistoryByTick = 1 << 2,
        DataFree = 1 << 3,
        NotDataFree = 1 << 4,
        StorageLimit255 = 1 << 5,
        StorageLimit65535 = 1 << 6,
        WithoutSparseDirectDense = 1 << 7,
        NoCompileFastAccess = 1 << 8,
        CompileSortAtLast = 1 << 9,
        UseISerialize = 1 << 10,
    }
}
