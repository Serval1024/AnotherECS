using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using static UnityEditor.Experimental.GraphView.Port;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct RecycleStorageFeature<TAllocator, TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IIdAllocator<TAllocator, TSparse, TDense, TDenseIndex>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDepencies depencies)
        {
            layout.storage.recycle.Allocate(allocator, depencies.config.general.recycleCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            => LayoutActions.GetCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<TNumberProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies)
            where TNumberProvider : struct, INumberProvier<TDenseIndex>
        {
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;

            if (recycleIndex > 0)
            {
                return storage.recycle.Get(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
#if !ANOTHERECS_RELEASE
                LayoutActions.CheckDenseLimit<TAllocator, TSparse, TDense, TDenseIndex, TSparse>(ref layout);
#endif
                return default(TNumberProvider).ToGeneric(denseIndex++);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DeallocateId(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, TDenseIndex id)
        {
            ref var recycleIndex = ref layout.storage.recycleIndex;
            ref var recycle = ref layout.storage.recycle;
            if (recycleIndex == recycle.Length)
            {
                recycle.Resize(recycle.Length << 1);
            }

            recycle.GetRef(recycleIndex++) = id;
        }
    }
}
