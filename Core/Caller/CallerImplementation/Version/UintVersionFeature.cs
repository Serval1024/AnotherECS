﻿using System.Runtime.CompilerServices;

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
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(JSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            layout.storage.version.Allocate(allocator, layout.storage.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.version.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint capacity)
        {
            layout.storage.version.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, ref GlobalDepencies depencies, uint index)
        {
            layout.storage.version.Set(index, depencies.tickProvider.tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, uint id)
            => layout.storage.version.Get(id);

        public unsafe void DropChange(ref UnmanagedLayout<TAllocator, TSparse, TDense, uint> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
        {
            var tick = depencies.tickProvider.tick;
            var versionPtr = layout.storage.version.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }
    }
}