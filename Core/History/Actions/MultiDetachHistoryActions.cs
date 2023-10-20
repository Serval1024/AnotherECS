using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal static class MultiDetachHistoryActions<T>
        where T : unmanaged, IDetach
    {
        /*
        public static unsafe void CallDetach_empty(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.REMOVE) != 0)
                {
                    default(T).OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallDetach_bool(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<bool>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.REMOVE) != 0)
                {
                    densePtr[i].OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallDetach_byte(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<byte>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.REMOVE) != 0)
                {
                    densePtr[sparsePtr[i]].OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallDetach_ushort(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<ushort>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.REMOVE) != 0)
                {
                    densePtr[sparsePtr[i]].OnDetach(state);
                }
            }
        }*/
    }
}