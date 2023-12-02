using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct ArchetypeDenseFeature<TSparse, TDense, TTickData> :
        ILayoutAllocator<TSparse, TDense, uint, TTickData>,
        ISparseResize<TSparse, TDense, uint, TTickData>,
        IDenseResize<TSparse, TDense, uint, TTickData>,
        IStartIndexProvider,
        IDenseProvider<TSparse, TDense, uint, TTickData>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, ref GlobalDepencies depencies)
        {
            var allocated = depencies.componentTypesCount + GetIndex();
            ref var storage = ref layout.storage;
            storage.denseIndex = allocated;
            storage.dense.Allocate(allocated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint capacity)
        {
            layout.storage.dense.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint index)
            => ref layout.storage.dense.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TDense* GetDensePtr(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint index)
            => layout.storage.dense.GetPtr(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout)
            => layout.storage.dense.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout)
            => layout.storage.denseIndex;
    }
}
