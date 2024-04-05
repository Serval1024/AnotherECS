using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SingleCF<TAllocator, TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IStartIndexProvider,
        IDenseProvider<TAllocator, TSparse, TDense, TDenseIndex>

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
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.dense.Allocate(allocator, 1);
            layout.denseIndex = GetIndex() + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense ReadDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index)
            => ref layout.dense.ReadRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index)
            => ref layout.dense.GetRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> ReadDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => new(layout.dense.ReadPtr(), layout.dense.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> GetDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => new(layout.dense.GetPtr(), layout.dense.Length);
    }
}

