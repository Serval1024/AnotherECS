using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RepairMemoryCF<TDense> : IRepairMemory<TDense>, IBoolConst
        where TDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairMemory(ref ComponentFunction<TDense> componentFunction, ref RepairMemoryContext repairMemoryContext, ref TDense component)
        {
            componentFunction.repairMemory(ref repairMemoryContext, ref component);
        }
    }

    internal unsafe struct RepairMemoryIterator<TDense> : IDataIterator<TDense>
        where TDense : unmanaged
    {
        public RepairMemoryFunctionData<TDense> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(uint index, ref TDense component)
        {
            default(RepairMemoryCF<TDense>)
                .RepairMemory(ref data.componentFunction, ref data.repairMemoryContext, ref component);
        }
    }
}