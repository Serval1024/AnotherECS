using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DeconstructInjectIterator<TAllocator, TSparse, TDense, TDenseIndex> : IDataIterator<TDense>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public InjectData<TDense> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(uint index, ref TDense component)
        {
            InjectCF<TAllocator, TSparse, TDense, TDenseIndex> injectFeature = default;
            injectFeature.Deconstruct(ref data.componentFunction, ref *data.dependencies, ref component);
        }
    }
}
