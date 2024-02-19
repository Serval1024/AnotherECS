using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SingleSparseCF<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, bool, TDense, uint>,
        ISparseResize<TAllocator, bool, TDense, uint>,
        IDenseResize<TAllocator, bool, TDense, uint>,
        ISparseProvider<TAllocator, bool, TDense, uint>,
        IDataIterable<TAllocator, bool, TDense, uint>,
        IBoolConst,
        ISingleDenseFlag,
        IDisposable

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        private MockSparseProvider _mockSparseProvider;

        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
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
           => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, bool, TDense, uint> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.sparse.Allocate(allocator, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, bool, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, bool, TDense, uint> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id)
            => layout.sparse.Read(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TIterator>(ref ULayout<TAllocator, bool, TDense, uint> layout, ref TIterator iterator, uint startIndex, uint count)
            where TIterator : struct, IDataIterator<TDense>
        {
            if (layout.sparse.Read(0))
            {
                iterator.Each(0, ref layout.dense.GetRef(0));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TDense> GetEnumerable(ULayout<TAllocator, bool, TDense, uint> layout, uint startIndex, uint count)
        {
            if (layout.sparse.Read(0))
            {
                yield return layout.dense.GetRef(0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool ReadSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, EntityId id)
            => ref layout.sparse.ReadRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, EntityId id)
            => ref layout.sparse.GetRef(0);

        public void SetSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id, uint denseIndex)
        {
            layout.sparse.GetRef(0) = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, bool, TDense, uint> layout, ref Dependencies dependencies)
            where T : unmanaged
        {
            if (typeof(T) == typeof(ushort))
            {
                return _mockSparseProvider.Get<T>(layout.sparse.Length);
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
