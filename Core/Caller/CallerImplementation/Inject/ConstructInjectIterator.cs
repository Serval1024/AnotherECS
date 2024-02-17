using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct ConstructInjectIterator<TAllocator, TSparse, TDense, TDenseIndex> : IDataIterator<TDense>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public InjectData<TDense> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(uint index, ref TDense component)
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
