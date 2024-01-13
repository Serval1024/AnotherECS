using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct IncrementStorageFeature<TAllocator, TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IIdAllocator<TAllocator, TSparse, TDense, TDenseIndex>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref Dependencies dependencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            => LayoutActions.GetSpaceCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<TNumberProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies)
            where TNumberProvider : struct, INumberProvier<TDenseIndex>
        {
            ref var denseIndex = ref layout.denseIndex;
#if !ANOTHERECS_RELEASE
            LayoutActions.CheckDenseLimit<TAllocator, TSparse, TDense, TDenseIndex, TSparse>(ref layout);
#endif
            return default(TNumberProvider).ToGeneric(denseIndex++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, TDenseIndex id) { }
    }
}
