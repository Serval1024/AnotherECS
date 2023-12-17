﻿using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class DetachLayoutActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_bool<TAllocator, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex> layout, State state, uint startIndex)
            where TAllocator : unmanaged, IAllocator
            where TDenseIndex : unmanaged
            where TDense : unmanaged, IDetach
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr();
            var dense = storage.dense.GetPtr();
            var denseIndex = storage.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    dense[i].OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_ushort<TAllocator, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex> layout, State state, uint count)
            where TAllocator : unmanaged, IAllocator
            where TDenseIndex : unmanaged
            where TDense : unmanaged, IDetach
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr();
                var sparseLength = storage.sparse.Length;
                var dense = storage.dense.GetPtr();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        dense[sparse[i]].OnDetach(state);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Detach_bool<TAllocator, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex> layout, State state, ref NArray<BAllocator, Op> ops)
            where TAllocator : unmanaged, IAllocator
            where TDense : unmanaged, IDetach
            where TDenseIndex : unmanaged
        {
            ref var sparse = ref layout.storage.sparse;
            var densePtr = layout.storage.dense.GetPtr();

            for (uint i = 0, iMax = sparse.Length; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.ADD) != 0)
                {
                    densePtr[i].OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Detach_ushort<TAllocator, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, ushort, TDense, TDenseIndex> layout, State state, ref NArray<BAllocator, Op> ops)
            where TAllocator : unmanaged, IAllocator
            where TDense : unmanaged, IDetach
            where TDenseIndex : unmanaged
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr();
            var densePtr = layout.storage.dense.GetPtr();
            var opsPtr = ops.GetPtr();

            for (uint i = 0, iMax = sparse.Length; i < iMax; ++i)
            {
                if ((opsPtr[i] & Op.ADD) != 0)
                {
                    densePtr[sparsePtr[i]].OnDetach(state);
                }
            }
        }
    }
}

