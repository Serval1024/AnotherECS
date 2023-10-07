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
        HistoryByVersion = 1 << 4,
        DataFree = 1 << 5,
        NotDataFree = 1 << 6,
        StorageLimit255 = 1 << 7,
        StorageLimit65535 = 1 << 8,
        WithoutSparseDirectDense = 1 << 9,
        NoCompileFastAccess = 1 << 10,
        CompileSortAtLast = 1 << 11,
        UseISerialize = 1 << 112,
    }
}
