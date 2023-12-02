using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal struct UintVersionFeature<TSparse, TDense, TTickData> :
        ILayoutAllocator<TSparse, TDense, uint, TTickData>,
        ISparseResize<TSparse, TDense, uint, TTickData>,
        IDenseResize<TSparse, TDense, uint, TTickData>,
        IChange<TSparse, TDense, uint, TTickData>,
        IVersion<TSparse, TDense, uint, TTickData>,
        IRevertFinished,
        IBoolConst

        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsRevertFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => default(JSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.version.Allocate(layout.storage.dense.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.version.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint capacity)
        {
            layout.storage.version.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint index)
        {
            layout.storage.version.Set(index, depencies.tickProvider.tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, uint id)
            => layout.storage.version.Get(id);

        public unsafe void DropChange(ref UnmanagedLayout<TSparse, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
        {
            var tick = depencies.tickProvider.tick;
            var versionPtr = layout.storage.version.GetPtr();
            for (uint i = startIndex; i < count; ++i)
            {
                versionPtr[i] = tick;
            }
        }
    }
}
