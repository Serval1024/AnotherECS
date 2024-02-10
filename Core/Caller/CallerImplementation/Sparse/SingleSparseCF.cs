using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct SingleSparseCF<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, bool, TDense, uint>,
        ISparseResize<TAllocator, bool, TDense, uint>,
        IDenseResize<TAllocator, bool, TDense, uint>,
        ISparseProvider<TAllocator, bool, TDense, uint>,
        IIterator<TAllocator, bool, TDense, uint>,
        IDataIterator<TAllocator, bool, TDense, uint>,
        IBoolConst,
        ISingleDenseFlag

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(State state, Dependencies* dependencies, ushort callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>
        { }

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
        public void ForEach<AIterable>(ref ULayout<TAllocator, bool, TDense, uint> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, bool, TDense, uint>
        {
            if (layout.sparse.ReadPtr()[0])
            {
                default(AIterable).Each(ref layout, ref dependencies, ref layout.dense.GetRef(0));
            }
        }
        public void ForEach<AIterable, TEachData>(ref ULayout<TAllocator, bool, TDense, uint> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TDense, TEachData>
            where TEachData : struct
        {
            if (layout.sparse.ReadPtr()[0])
            {
                default(AIterable).Each(ref data, 0, ref layout.dense.GetRef(0));
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
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, bool, TDense, uint> layout)
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
