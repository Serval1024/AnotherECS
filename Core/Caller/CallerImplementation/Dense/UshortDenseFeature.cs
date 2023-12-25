﻿using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

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
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(allocator, depencies.config.general.componentCapacity);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            TSparseBoolConst sparseBoolConst = default;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> GetDense<T>(ref UnmanagedLayout<TAllocator, TSparse, TDense, ushort> layout)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            if (typeof(T) == typeof(TDense))
#endif
            {
                return new WArray<T>((T*)layout.storage.dense.GetPtr(), layout.storage.dense.Length);
            }
#if !ANOTHERECS_RELEASE
            throw new System.ArgumentException(nameof(T));
#endif
        }
    }
}
