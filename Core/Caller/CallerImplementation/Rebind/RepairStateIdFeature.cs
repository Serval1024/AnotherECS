using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RepairStateIdFeature<TDense> : IRepairStateId<TDense>, IBoolConst
       where TDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairStateId(ref ComponentFunction<TDense> componentFunction, ushort stateId, ref TDense component)
        {
            componentFunction.repairStateId(stateId, ref component);
        }
    }

    internal unsafe struct RepairStateIdIterable<TDense> : IDataIterable<TDense, ComponentFunctionData<TDense>>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref ComponentFunctionData<TDense> data, uint index, ref TDense component)
        {
            default(RepairStateIdFeature<TDense>)
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