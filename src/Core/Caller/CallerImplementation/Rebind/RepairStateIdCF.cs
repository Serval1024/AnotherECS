using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RepairStateIdCF<TDense> : IRepairStateId<TDense>, IBoolConst
       where TDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairStateId(ref ComponentFunction<TDense> componentFunction, ushort stateId, ref TDense component)
        {
            componentFunction.repairStateId(stateId, ref component);
        }
    }

    internal unsafe struct RepairStateIdIterator<TDense> : IDataIterator<TDense>
        where TDense : unmanaged
    {
        public ComponentFunctionData<TDense> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(uint index, ref TDense component)
        {
            default(RepairStateIdCF<TDense>)
                .RepairStateId(ref data.componentFunction, data.dependencies->stateId, ref component);
        }
    }

    internal unsafe struct RepairStateIdData<TDense>
        where TDense : unmanaged
    {
        public Dependencies* dependencies;
        public ComponentFunction<TDense> componentFunction;
    }
}