using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UshortSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<ushort, TDense, ushort, TTickData>,
        ISparseResize<ushort, TDense, ushort, TTickData>,
        IDenseResize<ushort, TDense, ushort, TTickData>,
        ISparseProvider<ushort, TDense, ushort, TTickData, TTickDataDense>,
        IIterator<ushort, TDense, ushort, TTickData>,
        IBoolConst,
        ISingleDenseFlag

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
           where JSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
            => layout.storage.sparse.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
           => layout.storage.sparse.Get(id) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<ushort, TDense, ushort, TTickData>
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr();
                var sparseLength = storage.sparse.Length;
                var dense = storage.dense.GetPtr();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        iterable.Each(ref layout, ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse<THistory>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, EntityId id, ushort denseIndex)
            where THistory : struct, IHistory<ushort, TDense, ushort, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();

            THistory history = default;
            history.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ushort GetSparse(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
            => ref layout.storage.sparse.GetRef(id);
    }
}
