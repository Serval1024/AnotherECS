using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct UshortDenseFeature<TSparse, TDense, TTickData> :
        ILayoutAllocator<TSparse, TDense, ushort, TTickData>,
        ISparseResize<TSparse, TDense, ushort, TTickData>,
        IDenseResize<TSparse, TDense, ushort, TTickData>,
        IStartIndexProvider,
        IDenseProvider<TSparse, TDense, ushort, TTickData>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(JSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(depencies.config.general.componentCapacity);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.dense.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
        {
            layout.storage.dense.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ushort index)
            => ref layout.storage.dense.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TDense* GetDensePtr(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ushort index)
            => layout.storage.dense.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout)
            => layout.storage.dense.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout)
            => layout.storage.denseIndex;
    }
}
