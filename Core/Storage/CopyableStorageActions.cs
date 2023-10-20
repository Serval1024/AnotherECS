using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class SingleCopyableStorageActions<T>
        where T : unmanaged, ICopyable<T>
    {
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallRecycle_bool(ref UnmanagedLayout<T> layout)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            if (*sparse)
            {
                dense->OnRecycle();
            }
        }*/
    }

    internal static unsafe class MultiCopyableStorageActions<T>
        where T : unmanaged, ICopyable<T>
    { 
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallRecycle_bool(ref UnmanagedLayout<T> layout)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();
            var denseIndex = storage.denseIndex;

            for (uint i = 1; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    dense[i].OnRecycle();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallRecycle_byte(ref UnmanagedLayout<T> layout, uint count)
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
                        dense[sparse[i]].OnRecycle();
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallRecycle_ushort(ref UnmanagedLayout<T> layout, uint count)
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
                        dense[sparse[i]].OnRecycle();
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }*/
    }


}

