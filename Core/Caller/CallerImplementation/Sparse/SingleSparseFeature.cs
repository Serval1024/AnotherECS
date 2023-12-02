using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SingleSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<bool, TDense, uint, TTickData>,
        ISparseResize<bool, TDense, uint, TTickData>,
        IDenseResize<bool, TDense, uint, TTickData>,
        ISparseProvider<bool, TDense, uint, TTickData, TTickDataDense>,
        IIterator<bool, TDense, uint, TTickData>,
        IBoolConst,
        ISingleDenseFlag

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
           where JSparseBoolConst : struct, IBoolConst
           => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => layout.storage.sparse.Get(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<bool, TDense, uint, TTickData>
        {
            AIterable iterable = default;
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr();
            var dense = storage.dense.GetPtr();

            if (sparse[0])
            {
                iterable.Each(ref layout, ref depencies, ref dense[0]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse<THistory>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, EntityId id, uint denseIndex)
            where THistory : struct, IHistory<bool, TDense, uint, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();

            THistory history = default;
            history.PushSparse(ref layout, ref depencies, 0, sparse[0]);
            sparse[0] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, EntityId id)
            => ref layout.storage.sparse.GetRef(0);
    }
}
