using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal static class CopyableHistoryFacadeActions<T>
        where T : unmanaged, ICopyable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint offset, ref T data)
        {
            CopyableHistoryActions<T>.PushDense(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushFullDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            CopyableHistoryActions<T>.PushFullDense(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, ref layout.storage.dense);
        }
    }
}