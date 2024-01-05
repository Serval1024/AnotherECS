using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NonSparseFeature<TAllocator, TDense> :
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
        private ushort _itemId;
        private GlobalDependencies* _dependencies;

        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDependencies* dependencies, ushort callerId)
        {
            _dependencies = dependencies;
            _itemId = callerId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, TAllocator* allocator, ref GlobalDependencies dependencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, uint id)
            => _dependencies->archetype.IsHasItem(_dependencies->entities.ReadArchetypeId(id), _itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, ref GlobalDependencies dependencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, bool, TDense, uint>
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;
                var dense = storage.dense;

                dense.Dirty();
                for (uint i = startIndex, iMax = startIndex + count; i < iMax; ++i)
                {
                    iterable.Each(ref layout, ref dependencies, ref dense.ReadRef(i));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable, TEachData>(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TAllocator, bool, TDense, uint, TEachData>
            where TEachData : struct, IEachData
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;
                var dense = storage.dense;

                dense.Dirty();
                for (uint i = startIndex, iMax = startIndex + count; i < iMax; ++i)
                {
                    iterable.Each(ref data, i, ref dense.ReadRef(i));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, ref GlobalDependencies dependencies, EntityId id, uint denseIndex) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout, EntityId id)
            => throw new NotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref UnmanagedLayout<TAllocator, bool, TDense, uint> layout)
            where T : unmanaged
            => throw new NotSupportedException();
    }
}
