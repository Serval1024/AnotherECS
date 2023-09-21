using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class SingleAttachStorageActions<T>
        where T : unmanaged, IAttach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Attach_bool(ref UnmanagedLayout<T> layout, State state)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            if (*sparse)
            {
                dense->OnAttach(state);
            }
        }
    }

    internal static unsafe class SingleDetachStorageActions<T>
       where T : unmanaged, IDetach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_bool(ref UnmanagedLayout<T> layout, State state)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            if (*sparse)
            {
                dense->OnDetach(state);
            }
        }
    }

    internal static unsafe class DetachStorageActions<T>
       where T : unmanaged, IDetach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_empty(ref GlobalDepencies depencies, State state)
        {
            default(T).OnDetach(state);
        }
    }

    internal static unsafe class AttachStorageActions<T>
        where T : unmanaged, IAttach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Attach_empty(ref GlobalDepencies depencies, State state)
        {
            default(T).OnAttach(state);
        }
    }

    internal static unsafe class MultiAttachStorageActions<T>
        where T : unmanaged, IAttach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Attach_bool(ref UnmanagedLayout<T> layout, State state)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();
            var denseIndex = storage.denseIndex;

            for (uint i = 1; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    dense[i].OnAttach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Attach_byte(ref UnmanagedLayout<T> layout, State state, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<byte>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        dense[sparse[i]].OnAttach(state);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Attach_ushort(ref UnmanagedLayout<T> layout, State state, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<ushort>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        dense[sparse[i]].OnAttach(state);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    internal static unsafe class MultiDetachStorageActions<T>
        where T : unmanaged, IDetach
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_bool(ref UnmanagedLayout<T> layout, State state)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();
            var denseIndex = storage.denseIndex;

            for (uint i = 1; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    dense[i].OnDetach(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Detach_byte(ref UnmanagedLayout<T> layout, State state, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<byte>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

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
        public static void Detach_ushort(ref UnmanagedLayout<T> layout, State state, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<ushort>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

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
    }
}

