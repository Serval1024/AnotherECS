using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UshortSparseFeature<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, ushort, TDense, ushort>,
        ISparseResize<TAllocator, ushort, TDense, ushort>,
        IDenseResize<TAllocator, ushort, TDense, ushort>,
        ISparseProvider<TAllocator, ushort, TDense, ushort>,
        IIterator<TAllocator, ushort, TDense, ushort>,
        IDataIterator<TAllocator, ushort, TDense, ushort>,
        IBoolConst,
        ISingleDenseFlag

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(allocator, depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, uint id)
            => layout.storage.sparse.Read(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, uint id)
           => layout.storage.sparse.Read(id) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, ushort, TDense, ushort>
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;

                var sparse = storage.sparse.ReadPtr();
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
        public void ForEach<AIterable, TEachData>(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TAllocator, ushort, TDense, ushort, TEachData>
            where TEachData : struct, IEachData
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;

                var sparse = storage.sparse.ReadPtr();
                var sparseLength = storage.sparse.Length;
                var dense = storage.dense.GetPtr();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    var denseIndex = sparse[i];
                    if (denseIndex != 0)
                    {
                        iterable.Each(ref data, denseIndex, ref dense[denseIndex]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, ref GlobalDepencies depencies, EntityId id, ushort denseIndex)
        {
            layout.storage.sparse.GetPtr()[id] = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ushort GetSparse(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout, uint id)
            => ref layout.storage.sparse.GetRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref UnmanagedLayout<TAllocator, ushort, TDense, ushort> layout)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (typeof(T) == typeof(ushort))
#endif
            {
                return new WArray<T>((T*)layout.storage.sparse.ReadPtr(), layout.storage.sparse.Length);
            }
#if !ANOTHERECS_RELEASE
            throw new System.ArgumentException(nameof(T));
#endif
        }
    }
}
