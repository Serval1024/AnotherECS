using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static unsafe class SingleDetachHistoryActions<T>
        where T : unmanaged, IDetach
    {
        public static unsafe void CallDetach_empty(ref UnmanagedLayout<T> layout, State state, ref Op op)
        {
            ref var sparse = ref layout.storage.sparse;

            if ((op & Op.REMOVE) != 0)
            {
                default(T).OnDetach(state);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallDetach_bool(ref UnmanagedLayout<T> layout, State state, ref Op op)
        {
            var densePtr = layout.storage.dense.GetPtr<T>();

            if ((op & Op.REMOVE) != 0)
            {
                densePtr->OnDetach(state);
            }
        }
    }
}