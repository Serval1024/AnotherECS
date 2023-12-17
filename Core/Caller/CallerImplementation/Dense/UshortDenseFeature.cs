﻿using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UshortDenseFeature<TAllocator, TSparse, TDense> :
        ILayoutAllocator<TAllocator, TSparse, TDense, ushort>,
        ISparseResize<TAllocator, TSparse, TDense, ushort>,
        IDenseResize<TAllocator, TSparse, TDense, ushort>,
        IStartIndexProvider,
        IDenseProvider<TAllocator, TSparse, TDense, ushort>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged        
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(JSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(allocator, depencies.config.general.componentCapacity);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.dense.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
        {
            layout.storage.dense.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, ushort index)
            => ref layout.storage.dense.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TDense* GetDensePtr(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, ushort index)
            => layout.storage.dense.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout)
            => layout.storage.dense.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout)
            => layout.storage.denseIndex;
    }
}