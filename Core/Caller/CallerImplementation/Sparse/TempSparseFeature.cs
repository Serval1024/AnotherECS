using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct TempSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<bool, TDense, uint, TTickData>,
        ISparseResize<bool, TDense, uint, TTickData>,
        IDenseResize<bool, TDense, uint, TTickData>,
        ISparseProvider<bool, TDense, uint, TTickData, TTickDataDense>,
        IIterator<bool, TDense, uint, TTickData>,
        IBoolConst,
        ISingleDenseFlag

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        private ushort _itemId;
        private GlobalDepencies* _depencies;

        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(GlobalDepencies* depencies, ushort callerId)
        {
            _depencies = depencies;
            _itemId = callerId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
           where JSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => _depencies->archetype.IsHasItem(_depencies->entities.ReadArchetypeId(id), _itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<bool, TDense, uint, TTickData>
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
        public void SetSparse<THistory>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, EntityId id, uint denseIndex)
            where THistory : struct, IHistory<bool, TDense, uint, TTickData, TTickDataDense> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, EntityId id)
            => throw new NotSupportedException();
    }
}
