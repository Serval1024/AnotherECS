using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RebindMemoryFeature<TAllocator, TSparse, TDense, TDenseIndex> : IRebindMemory<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebindMemory(ref ComponentFunction<TDense> componentFunction, ref MemoryRebinderContext rebinder, ref TDense component)
        {
            componentFunction.memoryRebind(ref rebinder, ref component);
        }
    }

    internal unsafe struct RebindMemoryIterable<TAllocator, TSparse, TDense, TDenseIndex> : IDataIterable<TDense, RebindMemoryData<TDense>>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref RebindMemoryData<TDense> data, uint index, ref TDense component)
        {
            default(RebindMemoryFeature<TAllocator, TSparse, TDense, TDenseIndex>)
                .RebindMemory(ref data.componentFunction, ref data.dependencies->currentMemoryRebinder, ref component);
        }
    }

    internal unsafe struct RebindMemoryData<TDense> : IEachData
        where TDense : unmanaged
    {
        public GlobalDependencies* dependencies;
        public ComponentFunction<TDense> componentFunction;
    }
}