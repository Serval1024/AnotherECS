using AnotherECS.Core.Allocators;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RepairMemoryFunctionData<TDense>
       where TDense : unmanaged
    {
        public RepairMemoryContext repairMemoryContext;
        public ComponentFunction<TDense> componentFunction;
    }
}