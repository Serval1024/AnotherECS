using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UshortVersionFeature<TAllocator, TSparse, TDense> :
        ILayoutAllocator<TAllocator, TSparse, TDense, ushort>,
        ISparseResize<TAllocator, TSparse, TDense, ushort>,
        IDenseResize<TAllocator, TSparse, TDense, ushort>,
        IChange<TAllocator, TSparse, TDense, ushort>,
        IVersion<TAllocator, TSparse, TDense, ushort>,
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
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.tickVersion.Allocate(allocator, layout.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            TSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.tickVersion.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
        {
            layout.tickVersion.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, ref Dependencies dependencies, ushort index)
        {
            layout.tickVersion.Set(index, dependencies.tickProvider.tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, uint id)
            => layout.tickVersion.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DropChange(ref ULayout<TAllocator, TSparse, TDense, ushort> layout, ref Dependencies dependencies, uint startIndex, uint count)
        {
            var tick = dependencies.tickProvider.tick;
            var versionPtr = layout.tickVersion.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion(ref ULayout<TAllocator, TSparse, TDense, ushort> layout)
            => new(layout.tickVersion.ReadPtr(), layout.tickVersion.Length);
    }
}
