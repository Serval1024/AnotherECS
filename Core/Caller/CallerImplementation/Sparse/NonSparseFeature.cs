using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    /*
    internal unsafe struct NonSparseFeature2<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, uint, TDense, uint>,
        ISparseResize<TAllocator, uint, TDense, uint>,
        IDenseResize<TAllocator, uint, TDense, uint>,
        ISparseProvider<TAllocator, uint, TDense, uint>,
        IIterator<TAllocator, uint, TDense, uint>,
        IBoolConst,
        ISingleDenseFlag

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
           where JSparseBoolConst : struct, IBoolConst
           => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, uint id)
           => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, uint, TDense, uint>
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;
                var dense = storage.dense;

                for (uint i = startIndex, iMax = startIndex + count; i < iMax; ++i)
                {
                    iterable.Each(ref layout, ref depencies, ref dense.GetRef(i));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, ref GlobalDepencies depencies, EntityId id, uint denseIndex) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint GetSparse(ref UnmanagedLayout<TAllocator, uint, TDense, uint> layout, uint id)
            => throw new NotSupportedException();
    }*/
}
