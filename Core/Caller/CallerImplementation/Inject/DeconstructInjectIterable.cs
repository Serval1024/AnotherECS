using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DeconstructInjectIterable<TAllocator, TSparse, TDense, TDenseIndex> : IIterable<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            InjectFeature<TAllocator, TSparse, TDense, TDenseIndex> injectFeature = default;
            injectFeature.Deconstruct(ref layout, ref depencies, ref component);
        }
    }
}
