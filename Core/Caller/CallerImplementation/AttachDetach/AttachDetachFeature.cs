using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachDetachFeature<TAllocator, TSparse, TDense, TDenseIndex> : IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex>, IData, IBoolConst, ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>, ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>, IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        private State state;
        private NArray<BAllocator, byte> _temp;

        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, GlobalDependencies* dependencies)
        {
            this.state = state;
            _temp = new NArray<BAllocator, byte>(&dependencies->bAllocator, dependencies->config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDependencies dependencies)
        {
            layout.storage.addRemoveVersion.Allocate(allocator, dependencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            if (default(TSparseBoolConst).Is)
            {
                layout.storage.addRemoveVersion.Resize(capacity);
                _temp.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
        {
            layout.storage.addRemoveVersion.Resize(capacity);
            _temp.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRemoveEvent(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id)
        {
            unchecked
            {
                ++layout.storage.addRemoveVersion.GetRef(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage1(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint denseAllocated)
        {
            UnsafeMemory.MemCopy(_temp.ReadPtr(), layout.storage.addRemoveVersion.ReadPtr(), denseAllocated * sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, byte> GetAddRemoveVersion()
            => _temp;
    }
}
