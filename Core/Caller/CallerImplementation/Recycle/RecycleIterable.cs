using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RecycleIterable<TSparse, TDense, TDenseIndex, TTickData> : IIterable<TSparse, TDense, TDenseIndex, TTickData>
       where TSparse : unmanaged
       where TDense : unmanaged, ICopyable<TDense>
       where TDenseIndex : unmanaged
       where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            CopyableFeature<TDense> copyableFeature = default;
            copyableFeature.Recycle(ref component);
        }
    }
}
