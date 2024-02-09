using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RepairMemoryFeature<TDense> : IRepairMemory<TDense>, IBoolConst
        where TDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairMemory(ref ComponentFunction<TDense> componentFunction, ref RepairMemoryContext repairMemoryContext, ref TDense component)
        {
            componentFunction.repairMemory(ref repairMemoryContext, ref component);
        }
    }

    internal unsafe struct RepairMemoryIterable<TDense> : IDataIterable<TDense, RepairMemoryFunctionData<TDense>>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref RepairMemoryFunctionData<TDense> data, uint index, ref TDense component)
        {
            default(RepairMemoryFeature<TDense>)
                .RepairMemory(ref data.componentFunction, ref data.repairMemoryContext, ref component);
        }
    }
}