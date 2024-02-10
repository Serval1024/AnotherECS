using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct ConstructInjectIterable<TAllocator, TSparse, TDense, TDenseIndex> : IDataIterable<TDense, InjectData<TDense>>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref InjectData<TDense> data, uint index, ref TDense component)
        {
            default(InjectCF<TAllocator, TSparse, TDense, TDenseIndex>)
                .Construct(ref data.componentFunction, ref *data.dependencies, ref component);
        }
    }

    internal unsafe struct InjectData<TDense>
        where TDense : unmanaged
    {
        public Dependencies* dependencies;
        public ComponentFunction<TDense> componentFunction;
    }
}
