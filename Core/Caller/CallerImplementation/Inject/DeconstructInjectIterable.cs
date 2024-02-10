using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DeconstructInjectIterable<TAllocator, TSparse, TDense, TDenseIndex> : IDataIterable<TDense, InjectData<TDense>>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref InjectData<TDense> data, uint index, ref TDense component)
        {
            InjectCF<TAllocator, TSparse, TDense, TDenseIndex> injectFeature = default;
            injectFeature.Deconstruct(ref data.componentFunction, ref *data.dependencies, ref component);
        }
    }
}
