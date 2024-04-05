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
        DataFree = 1 << 2,
        NotDataFree = 1 << 3,
        WithoutSparseDirectDense = 1 << 4,
        ForceUseSparse = 1 << 5,
        CompileFastAccess = 1 << 6,
    }
}
