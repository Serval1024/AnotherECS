﻿using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SingleFeature<TAllocator, TSparse, TDense, TDenseIndex> :
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
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(allocator, 1);
            storage.denseIndex = GetIndex() + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index)
            => ref layout.storage.dense.GetRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TDense* GetDensePtr(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index)
            => layout.storage.dense.GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => 0;
    }
}