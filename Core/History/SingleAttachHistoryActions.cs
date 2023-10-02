using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static unsafe class SingleAttachHistoryActions<T>
      where T : unmanaged, IAttach
    {
        public static unsafe void CallAttach_empty(ref UnmanagedLayout<T> layout, State state, ref Op op)
        {
            ref var sparse = ref layout.storage.sparse;

            if ((op & Op.ADD) != 0)
            {
                default(T).OnAttach(state);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CallAttach_bool(ref UnmanagedLayout<T> layout, State state, ref Op op)
        {
            var densePtr = layout.storage.dense.GetPtr<T>();

            if ((op & Op.ADD) != 0)
            {
                densePtr->OnAttach(state);
            }
        }
    }
}