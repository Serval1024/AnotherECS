using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct CopyableFeature<TDense> : IDenseCopyable<TDense>, IBoolConst
        where TDense : unmanaged, ICopyable<TDense>
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ref TDense source, ref TDense destination)
        {
            destination.CopyFrom(in source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(ref TDense component)
        {
            component.OnRecycle();
            component = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle<TSparse, TDenseIndex, TTickData, TTickDataDense, TSparseStorage>
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TSparse : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
            where TSparseStorage : struct, IIterator<TSparse, TDense, TDenseIndex, TTickData>
        {
            default(TSparseStorage).ForEach<RecycleIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref layout, ref depencies, startIndex, count);
        }
    }
}
