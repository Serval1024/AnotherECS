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

        public ComponentOptionAttribute(ComponentOptions options, int capacity)
        {
            Options = options;
            Capacity = capacity;
        }
    }

    [Flags]
    public enum ComponentOptions
    {
        HistoryNonSync = 1 << 0,
        HistoryByChange = 1 << 1,
        HistoryByTick = 1 << 2,
        DataFree = 1 << 3,
        DataNotFree = 1 << 4,
        Blittable = 1 << 5,
        StorageLimit255 = 1 << 6,
        ExceptSparseDirectDense = 1 << 10,
        NoCompileDirectAccess = 1 << 11,
        CompileSortAtLast = 1 << 12,
        Capacity = 1 << 13,
        ForceUseISerialize = 1 << 14,
        ReferenceStorage = 1 << 15,
    }
}
