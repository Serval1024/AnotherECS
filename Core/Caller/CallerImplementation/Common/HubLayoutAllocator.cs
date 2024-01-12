using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct HubLayoutAllocator<
        TAllocator, TSparse, TDense, TDenseIndex,
        TAllocator0, TAllocator1, TAllocator2, TAllocator3
        > :

        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged

        where TAllocator0 : struct, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator1 : struct, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator2 : struct, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator3 : struct, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TAllocator0).IsSparseResize<TSparseBoolConst>() ||
                default(TAllocator1).IsSparseResize<TSparseBoolConst>() ||
                default(TAllocator2).IsSparseResize<TSparseBoolConst>() ||
                default(TAllocator3).IsSparseResize<TSparseBoolConst>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(
            ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout,
            ref GlobalDependencies dependencies,

            TAllocator* allocator0,
            TAllocator* allocator1,
            TAllocator* allocator2,
            TAllocator* allocator3
            )
        {
            default(TAllocator0).LayoutAllocate(ref layout, allocator0, ref dependencies);
            default(TAllocator1).LayoutAllocate(ref layout, allocator1, ref dependencies);
            default(TAllocator2).LayoutAllocate(ref layout, allocator2, ref dependencies);
            default(TAllocator3).LayoutAllocate(ref layout, allocator3, ref dependencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            default(TAllocator0).SparseResize<TSparseBoolConst>(ref layout, capacity);
            default(TAllocator1).SparseResize<TSparseBoolConst>(ref layout, capacity);
            default(TAllocator2).SparseResize<TSparseBoolConst>(ref layout, capacity);
            default(TAllocator3).SparseResize<TSparseBoolConst>(ref layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
        {
            default(TAllocator0).DenseResize(ref layout, capacity);
            default(TAllocator1).DenseResize(ref layout, capacity);
            default(TAllocator2).DenseResize(ref layout, capacity);
            default(TAllocator3).DenseResize(ref layout, capacity);            
        }
    }
}
