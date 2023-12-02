using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NonSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<uint, TDense, uint, TTickData>,
        ISparseResize<uint, TDense, uint, TTickData>,
        IDenseResize<uint, TDense, uint, TTickData>,
        ISparseProvider<uint, TDense, uint, TTickData, TTickDataDense>,
        IIterator<uint, TDense, uint, TTickData>,
        IBoolConst,
        ISingleDenseFlag

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
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
        public void Allocate(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, uint id)
           => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<uint, TDense, uint, TTickData>
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
        public void SetSparse<THistory>(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, EntityId id, uint denseIndex)
            where THistory : struct, IHistory<uint, TDense, uint, TTickData, TTickDataDense> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint GetSparse(ref UnmanagedLayout<uint, TDense, uint, TTickData> layout, uint id)
            => throw new NotSupportedException();
    }
}
