using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct EmptyFeature<TSparse, TDense, TDenseIndex, TTickData> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseProvider<TSparse, TDense, TDenseIndex, TTickData>,
        IStartIndexProvider

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(1);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, TDenseIndex index)
            => ref layout.storage.dense.GetRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TDense* GetDensePtr(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, TDenseIndex index)
            => layout.storage.dense.GetPtr(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            => 0;
    }
}
