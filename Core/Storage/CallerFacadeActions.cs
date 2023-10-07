using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class CallerFacadeActions<QSparse, WDense>
        where QSparse : unmanaged
        where WDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDenseMulti(ref UnmanagedLayout<QSparse, WDense> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            AllocateDense(ref layout, ref depencies, historyMode, depencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSparseMulti<USparse>(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
            where USparse : unmanaged
        {
            MultiStorageActions<T>.AllocateSparse<USparse>(ref layout, depencies.config.general.entityCapacity);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateSparse<USparse>(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateRecycleMulti(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateRecycle(ref layout, depencies.config.general.recycledCapacity);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateRecycle(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateVersion(ref layout, depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDenseSingle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            AllocateDense(ref layout, ref depencies, historyMode, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSparseSingle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateSparse<bool>(ref layout, 1);
            if (historyMode != HistoryMode.NONE)
            {
                MultiHistoryFacadeActions<T>.AllocateSparse<bool>(ref layout, ref depencies);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateVersionSingle(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, HistoryMode historyMode)
        {
            MultiStorageActions<T>.AllocateVersion(ref layout, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDense(ref UnmanagedLayout<QSparse, WDense> layout, ref GlobalDepencies depencies, HistoryMode historyMode, uint capacity)
        {
            MultiStorageActions<T>.AllocateDense(ref layout, capacity);
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
                case HistoryMode.BYVERSION:
                    {
                        MultiHistoryFacadeActions<T>.AllocateForVersionDense(ref layout, ref depencies);
                        break;
                    }
            }
        }
    }

    internal enum HistoryMode
    {
        NONE,
        BYCHANGE,
        BYTICK,
        BYVERSION
    }

}

