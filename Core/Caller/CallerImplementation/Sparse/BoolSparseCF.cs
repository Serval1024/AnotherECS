using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct BoolSparseCF<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, bool, TDense, ushort>,
        ISparseResize<TAllocator, bool, TDense, ushort>,
        IDenseResize<TAllocator, bool, TDense, ushort>,
        ISparseProvider<TAllocator, bool, TDense, ushort>,
        IIterable<TAllocator, bool, TDense, ushort>,
        IDataIterable<TAllocator, bool, TDense, ushort>,
        IBoolConst,
        ISingleDenseFlag,
        IDisposable

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        private MockSparseProvider _mockSparseProvider;

        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(Dependencies* dependencies, State state, uint callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>
        {
            _mockSparseProvider = new MockSparseProvider(&dependencies->bAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, bool, TDense, ushort> layout, TAllocator* allocator, ref Dependencies dependencies)
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
        public void ForEach<IIterator>(ref ULayout<TAllocator, bool, TDense, ushort> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where IIterator : struct, IIterator<TAllocator, bool, TDense, ushort>
        {
            IIterator iterator = default;

            var sparse = layout.sparse.ReadPtr();
            var dense = layout.dense.GetPtr();
            var denseIndex = layout.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterator.Each(ref layout, ref dependencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TIterator>(ref ULayout<TAllocator, bool, TDense, ushort> layout, ref TIterator iterator, uint startIndex, uint count)
            where TIterator : struct, IDataIterator<TDense>
        {
            var sparse = layout.sparse.ReadPtr();
            var dense = layout.dense.GetPtr();
            var denseIndex = layout.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterator.Each(i, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool ReadSparse(ref ULayout<TAllocator, bool, TDense, ushort> layout, EntityId id)
            => ref layout.sparse.ReadRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref ULayout<TAllocator, bool, TDense, ushort> layout, EntityId id)
            => ref layout.sparse.GetRef(id);

        public void SetSparse(ref ULayout<TAllocator, bool, TDense, ushort> layout, uint id, ushort denseIndex)
        {
            layout.sparse.GetRef(id) = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, bool, TDense, ushort> layout, ref Dependencies dependencies)
            where T : unmanaged
        {
            if (typeof(T) == typeof(ushort))
            {
                var array = _mockSparseProvider.Get<ushort>(layout.sparse.Length);
                var sparse = layout.sparse;
                const ushort zero = 0;
                for (ushort i = 1, iMax = (ushort)sparse.Length; i < iMax; ++i)
                {
                    array.GetRef(i) = sparse.GetRef(i) ? i : zero;
                }
                return new((T*)array.GetPtr(), array.Length);
            }
            else if (typeof(T) == typeof(bool))
            {
                return new WArray<T>((T*)layout.sparse.ReadPtr(), layout.sparse.Length);
            }

            throw new ArgumentException(typeof(T).Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _mockSparseProvider.Dispose();
        }
    }
}
