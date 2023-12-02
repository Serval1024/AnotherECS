using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class DetachLayoutActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_bool<TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<bool, TDense, TDenseIndex, TTickData> layout, State state, uint startIndex)
            where TDenseIndex : unmanaged
            where TDense : unmanaged, IDetach
            where TTickData : unmanaged
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
        public static void Detach_ushort<TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData> layout, State state, uint count)
            where TDenseIndex : unmanaged
            where TDense : unmanaged, IDetach
            where TTickData : unmanaged
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
        public static unsafe void Detach_bool<TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<bool, TDense, TDenseIndex, TTickData> layout, State state, ref NArray<Op> ops)
          where TDense : unmanaged, IDetach
          where TDenseIndex : unmanaged
          where TTickData : unmanaged
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
        public static unsafe void Detach_ushort<TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData> layout, State state, ref NArray<Op> ops)
            where TDense : unmanaged, IDetach
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
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

