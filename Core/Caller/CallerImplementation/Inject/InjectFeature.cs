using AnotherECS.Core.Allocators;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct InjectFeature<TAllocator, TSparse, TDense, TDenseIndex> : IInject<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component)
        {
            componentFunction.construct(ref dependencies.injectContainer, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component)
        {
            componentFunction.deconstruct(ref dependencies.injectContainer, ref component);
        }
    }
}
