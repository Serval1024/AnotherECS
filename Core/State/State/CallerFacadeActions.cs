using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class CallerFacadeActions<T>
        where T : unmanaged
    {
        public enum HistoryMode
        {
            NONE,
            BYCHANGE,
            BYTICK,
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDenseMulti(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateDense(ref layout, depencies.config.componentCapacity);
            switch (historyMode)
            {
                case HistoryMode.BYCHANGE:
                    {
                        MultiHistoryFacadeActions<T>.AllocateDense(ref layout, ref depencies);
                        break;
                    }
                case HistoryMode.BYTICK:
                    {
                        MultiHistoryFacadeActions<T>.AllocateForFullDense(ref layout, ref depencies);
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSparseMulti<USparse>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
            where USparse : unmanaged
        {
            MultiStorageActions<T>.AllocateSparse<USparse>(ref layout, depencies.config.entityCapacity);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateSparse<USparse>(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateRecycleMulti(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateRecycle(ref layout, depencies.config.recycledCapacity);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateRecycle(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateVersion(ref layout, depencies.config.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSingleDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateDense(ref layout, 1);            

            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateDense(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSingleSparse(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateSparse<bool>(ref layout, 1);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateSparse<bool>(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSingleVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateVersion(ref layout, 1);
        }
    }
}

