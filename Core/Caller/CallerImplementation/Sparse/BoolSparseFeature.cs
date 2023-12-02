using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BoolSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<bool, TDense, ushort, TTickData>,
        ISparseResize<bool, TDense, ushort, TTickData>,
        IDenseResize<bool, TDense, ushort, TTickData>,
        ISparseProvider<bool, TDense, ushort, TTickData, TTickDataDense>,
        IIterator<bool, TDense, ushort, TTickData>,
        IBoolConst,
        ISingleDenseFlag

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
           where JSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, uint id)
            => (ushort)id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, uint id)
            => layout.storage.sparse.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<bool, TDense, ushort, TTickData>
        {
            AIterable iterable = default;
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr();
            var dense = storage.dense.GetPtr();
            var denseIndex = storage.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref layout, ref depencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse<THistory>(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, EntityId id, ushort denseIndex)
            where THistory : struct, IHistory<bool, TDense, ushort, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();

            THistory history = default;
            history.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<bool, TDense, ushort, TTickData> layout, EntityId id)
            => ref layout.storage.sparse.GetRef(id);
    }
}
