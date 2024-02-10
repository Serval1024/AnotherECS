using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UintDenseCF<TAllocator, TSparse, TDense> :
        ILayoutAllocator<TAllocator, TSparse, TDense, uint>,
        ISparseResize<TAllocator, TSparse, TDense, uint>,
        IDenseResize<TAllocator, TSparse, TDense, uint>,
        IStartIndexProvider,
        IDenseProvider<TAllocator, TSparse, TDense, uint>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, uint> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.dense.Allocate(allocator, dependencies.config.general.componentCapacity);
            layout.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            TSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.dense.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
        {
            layout.dense.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint index)
            => ref layout.dense.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense ReadDense(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint index)
            => ref layout.dense.ReadRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref ULayout<TAllocator, TSparse, TDense, uint> layout)
            => layout.dense.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref ULayout<TAllocator, TSparse, TDense, uint> layout)
            => layout.denseIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> ReadDense(ref ULayout<TAllocator, TSparse, TDense, uint> layout)
            => new(layout.dense.ReadPtr(), layout.dense.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> GetDense(ref ULayout<TAllocator, TSparse, TDense, uint> layout)
            => new(layout.dense.GetPtr(), layout.dense.Length);
    }
}
