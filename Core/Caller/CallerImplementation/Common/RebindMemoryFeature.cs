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
        public void RebindMemory(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref MemoryRebinderContext rebinder, ref TDense component)
        {
            layout.componentFunction.memoryRebind(ref rebinder, ref component);
        }
    }

    internal unsafe struct RebindMemoryIterable<TAllocator, TSparse, TDense, TDenseIndex> : IIterable<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDependencies dependencies, ref TDense component)
        {
            default(RebindMemoryFeature<TAllocator, TSparse, TDense, TDenseIndex>)
                .RebindMemory(ref layout, ref dependencies.currentMemoryRebinder, ref component);
        }
    }
}