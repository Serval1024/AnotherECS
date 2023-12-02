using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DeconstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData> : IIterable<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            InjectFeature<TSparse, TDense, TDenseIndex, TTickData> injectFeature = default;
            injectFeature.Deconstruct(ref layout, ref depencies, ref component);
        }
    }
}
