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
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            layout.storage.tickVersion.Allocate(allocator, layout.storage.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            TSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.tickVersion.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
        {
            layout.storage.tickVersion.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, ref GlobalDepencies depencies, ushort index)
        {
            layout.storage.tickVersion.Set(index, depencies.tickProvider.tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint id)
            => layout.storage.tickVersion.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DropChange(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
        {
            var tick = depencies.tickProvider.tick;
            var versionPtr = layout.storage.tickVersion.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout)
            => new(layout.storage.tickVersion.ReadPtr(), layout.storage.tickVersion.Length);
    }
}
