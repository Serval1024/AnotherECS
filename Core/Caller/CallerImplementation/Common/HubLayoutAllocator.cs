using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct HubLayoutAllocator<TSparse, TDense, TDenseIndex, TTickData, TAllocator0, TAllocator1, TAllocator2, TAllocator3, TAllocator4> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
        where TAllocator0 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator1 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator2 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator3 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator4 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(TAllocator0).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator1).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator2).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator3).IsSparseResize<JSparseBoolConst>() ||
                default(TAllocator4).IsSparseResize<JSparseBoolConst>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.Allocate(ref layout, ref depencies);
            allocator1.Allocate(ref layout, ref depencies);
            allocator2.Allocate(ref layout, ref depencies);
            allocator3.Allocate(ref layout, ref depencies);
            allocator4.Allocate(ref layout, ref depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator1.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator2.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator3.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator4.SparseResize<JSparseBoolConst>(ref layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.DenseResize(ref layout, capacity);
            allocator1.DenseResize(ref layout, capacity);
            allocator2.DenseResize(ref layout, capacity);
            allocator3.DenseResize(ref layout, capacity);
            allocator4.DenseResize(ref layout, capacity);
        }
    }
}
