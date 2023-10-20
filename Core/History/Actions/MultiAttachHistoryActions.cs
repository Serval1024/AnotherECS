using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal static class MultiAttachHistoryActions<T>
        where T : unmanaged, IAttach
    {
        /*
        public static unsafe void CallAttach_empty(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.ADD) != 0)
                {
                    default(T).OnAttach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallAttach_bool(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<bool>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.ADD) != 0)
                {
                    densePtr[i].OnAttach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallAttach_byte(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<byte>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.ADD) != 0)
                {
                    densePtr[sparsePtr[i]].OnAttach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallAttach_ushort(ref UnmanagedLayout<T> layout, State state, ref ArrayPtr<Op> ops)
        {
            ref var sparse = ref layout.storage.sparse;
            var sparsePtr = sparse.GetPtr<ushort>();
            var densePtr = layout.storage.dense.GetPtr<T>();

            for (uint i = 0, iMax = sparse.ElementCount; i < iMax; ++i)
            {
                if ((ops.Get(i) & Op.ADD) != 0)
                {
                    densePtr[sparsePtr[i]].OnAttach(state);
                }
            }
        }*/
    }
}