using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BoolSparseFeature<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, bool, TDense, ushort>,
        ISparseResize<TAllocator, bool, TDense, ushort>,
        IDenseResize<TAllocator, bool, TDense, ushort>,
        ISparseProvider<TAllocator, bool, TDense, ushort>,
        IIterator<TAllocator, bool, TDense, ushort>,
        IDataIterator<TAllocator, bool, TDense, ushort>,
        IBoolConst,
        ISingleDenseFlag

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDependencies* dependencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, bool, TDense, ushort> layout, TAllocator* allocator, ref GlobalDependencies dependencies)
        {
            layout.sparse.Allocate(allocator, dependencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, bool, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            layout.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, bool, TDense, ushort> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref ULayout<TAllocator, bool, TDense, ushort> layout, uint id)
            => (ushort)id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref ULayout<TAllocator, bool, TDense, ushort> layout, uint id)
            => layout.sparse.Read(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref ULayout<TAllocator, bool, TDense, ushort> layout, ref GlobalDependencies dependencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, bool, TDense, ushort>
        {
            AIterable iterable = default;

            var sparse = layout.sparse.ReadPtr();
            var dense = layout.dense.GetPtr();
            var denseIndex = layout.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref layout, ref dependencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable, TEachData>(ref ULayout<TAllocator, bool, TDense, ushort> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TDense, TEachData>
            where TEachData : struct, IEachData
        {
            AIterable iterable = default;

            var sparse = layout.sparse.ReadPtr();
            var dense = layout.dense.GetPtr();
            var denseIndex = layout.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref data, i, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse(ref ULayout<TAllocator, bool, TDense, ushort> layout, ref GlobalDependencies dependencies, EntityId id, ushort denseIndex)
        {
            layout.sparse.GetPtr()[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref ULayout<TAllocator, bool, TDense, ushort> layout, EntityId id)
            => ref layout.sparse.GetRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, bool, TDense, ushort> layout)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (typeof(T) == typeof(bool))
#endif
            {
                return new WArray<T>((T*)layout.sparse.ReadPtr(), layout.sparse.Length);
            }
#if !ANOTHERECS_RELEASE
            throw new System.ArgumentException(typeof(T).Name);
#endif
        }
    }
}
