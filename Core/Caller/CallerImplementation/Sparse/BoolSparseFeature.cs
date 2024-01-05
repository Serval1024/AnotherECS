﻿using AnotherECS.Core.Collection;
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
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, TAllocator* allocator, ref GlobalDependencies dependencies)
        {
            layout.storage.sparse.Allocate(allocator, dependencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, uint id)
            => (ushort)id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, uint id)
            => layout.storage.sparse.Read(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, ref GlobalDependencies dependencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, bool, TDense, ushort>
        {
            AIterable iterable = default;
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.ReadPtr();
            var dense = storage.dense.GetPtr();
            var denseIndex = storage.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref layout, ref dependencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable, TEachData>(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TAllocator, bool, TDense, ushort, TEachData>
            where TEachData : struct, IEachData
        {
            AIterable iterable = default;
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.ReadPtr();
            var dense = storage.dense.GetPtr();
            var denseIndex = storage.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref data, i, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, ref GlobalDependencies dependencies, EntityId id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();
            sparse[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout, EntityId id)
            => ref layout.storage.sparse.GetRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref UnmanagedLayout<TAllocator, bool, TDense, ushort> layout)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            if (typeof(T) == typeof(bool))
#endif
            {
                return new WArray<T>((T*)layout.storage.sparse.ReadPtr(), layout.storage.sparse.Length);
            }
#if !ANOTHERECS_RELEASE
            throw new System.ArgumentException(typeof(T).Name);
#endif
        }
    }
}
