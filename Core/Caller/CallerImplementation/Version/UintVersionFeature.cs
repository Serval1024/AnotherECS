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
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            layout.storage.tickVersion.Allocate(allocator, layout.storage.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            if (default(TSparseBoolConst).Is)
            {
                layout.storage.tickVersion.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
        {
            layout.storage.tickVersion.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, ref GlobalDepencies depencies, uint index)
        {
            layout.storage.tickVersion.GetRef(index) = depencies.tickProvider.tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint id)
            => layout.storage.tickVersion.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DropChange(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
        {
            var tick = depencies.tickProvider.tick;
            var versionPtr = layout.storage.tickVersion.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout)
            => new(layout.storage.tickVersion.ReadPtr(), layout.storage.tickVersion.Length);
    }
}
