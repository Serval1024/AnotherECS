using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UintVersionFeature<TAllocator, TSparse, TDense> :
        ILayoutAllocator<TAllocator, TSparse, TDense, uint>,
        ISparseResize<TAllocator, TSparse, TDense, uint>,
        IDenseResize<TAllocator, TSparse, TDense, uint>,
        IChange<TAllocator, TSparse, TDense, uint>,
        IVersion<TAllocator, TSparse, TDense, uint>,
        IRevertFinished,
        IBoolConst

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsRevertFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, uint> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.tickVersion.Allocate(allocator, layout.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            if (default(TSparseBoolConst).Is)
            {
                layout.tickVersion.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
        {
            layout.tickVersion.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref ULayout<TAllocator, TSparse, TDense, uint> layout, ref Dependencies dependencies, uint index)
        {
            layout.tickVersion.GetRef(index) = dependencies.tickProvider.tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref ULayout<TAllocator, TSparse, TDense, uint> layout, uint id)
            => layout.tickVersion.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DropChange(ref ULayout<TAllocator, TSparse, TDense, uint> layout, ref Dependencies dependencies, uint startIndex, uint count)
        {
            var tick = dependencies.tickProvider.tick;
            var versionPtr = layout.tickVersion.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion(ref ULayout<TAllocator, TSparse, TDense, uint> layout)
            => new(layout.tickVersion.ReadPtr(), layout.tickVersion.Length);
    }
}
