﻿using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct UshortSparseCF<TAllocator, TDense> :
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
        public void Config<TMemoryAllocatorProvider>(State state, Dependencies* dependencies, ushort callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, ushort, TDense, ushort> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            layout.sparse.Allocate(allocator, dependencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            layout.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint id)
            => layout.sparse.Read(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint id)
           => layout.sparse.Read(id) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TIterable>(ref ULayout<TAllocator, ushort, TDense, ushort> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where TIterable : struct, IIterable<TAllocator, ushort, TDense, ushort>
        {
            if (count != 0)
            {
                TIterable iterable = default;
                
                var sparse = layout.sparse.ReadPtr();
                var sparseLength = layout.sparse.Length;
                var dense = layout.dense.GetPtr();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        iterable.Each(ref layout, ref dependencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TIterable, TEachData>(ref ULayout<TAllocator, ushort, TDense, ushort> layout, TEachData data, uint startIndex, uint count)
            where TIterable : struct, IDataIterable<TDense, TEachData>
            where TEachData : struct
        {
            if (count != 0)
            {
                TIterable iterable = default;

                var sparse = layout.sparse.ReadPtr();
                var sparseLength = layout.sparse.Length;
                var dense = layout.dense.GetPtr();

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
        public ref ushort ReadSparse(ref ULayout<TAllocator, ushort, TDense, ushort> layout, EntityId id)
            => ref layout.sparse.ReadRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ushort GetSparse(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint id)
            => ref layout.sparse.GetRef(id);

        public void SetSparse(ref ULayout<TAllocator, ushort, TDense, ushort> layout, uint id, ushort denseIndex)
        {
            layout.sparse.GetRef(id) = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, ushort, TDense, ushort> layout)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (typeof(T) == typeof(ushort))
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