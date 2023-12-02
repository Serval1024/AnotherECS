using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct InjectFeature<TSparse, TDense, TDenseIndex, ETickDataDense> : IInject<TSparse, TDense, TDenseIndex, ETickDataDense>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where ETickDataDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, ETickDataDense> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            layout.componentFunction.construct(ref depencies.injectContainer, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, ETickDataDense> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            layout.componentFunction.deconstruct(ref depencies.injectContainer, ref component);
        }
    }
}
