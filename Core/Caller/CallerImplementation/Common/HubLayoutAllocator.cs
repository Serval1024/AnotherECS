using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct HubLayoutAllocator<
        TAllocator, TSparse, TDense, TDenseIndex,
        TAllocator0, TAllocator1, TAllocator2, TAllocator3
        > :

        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
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
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(TAllocator0).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator1).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator2).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator3).IsSparseResize<JSparseBoolConst>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            default(TAllocator0).Allocate(ref layout, allocator, ref depencies);
            default(TAllocator1).Allocate(ref layout, allocator, ref depencies);
            default(TAllocator2).Allocate(ref layout, allocator, ref depencies);
            default(TAllocator3).Allocate(ref layout, allocator, ref depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            default(TAllocator0).SparseResize<JSparseBoolConst>(ref layout, capacity);
            default(TAllocator1).SparseResize<JSparseBoolConst>(ref layout, capacity);
            default(TAllocator2).SparseResize<JSparseBoolConst>(ref layout, capacity);
            default(TAllocator3).SparseResize<JSparseBoolConst>(ref layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
        {
            default(TAllocator0).DenseResize(ref layout, capacity);
            default(TAllocator1).DenseResize(ref layout, capacity);
            default(TAllocator2).DenseResize(ref layout, capacity);
            default(TAllocator3).DenseResize(ref layout, capacity);
        }
    }
}
