using AnotherECS.Core.Actions;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachDetachExternalCF<TAllocator, TSparse, TDense, TDenseIndex> :
        IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex>,
        IData<TAllocator>,
        IBoolConst,
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IRepairMemoryHandle,
        ISerialize,
        IDisposable

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        private TAllocator* _allocator;
        private State state;
        private NContainer<BAllocator, NArray<BAllocator, byte>> _temp;
        private MemoryHandle _layoutMemoryHandle;
        private GenerationULayout<TAllocator>* _layout;

        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(State state, Dependencies* dependencies, uint callerId)
           where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>
        {
            this.state = state;
            _allocator = default(TMemoryAllocatorProvider).GetStage0(dependencies);
            _temp = new NContainer<BAllocator, NArray<BAllocator, byte>>(&dependencies->bAllocator, default);
            _temp.GetRef() = new NArray<BAllocator, byte>(&dependencies->bAllocator, dependencies->config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref Dependencies dependencies)
        {
            _layoutMemoryHandle = _allocator->Allocate((uint)sizeof(GenerationULayout<TAllocator>));
            _layout = GetLayoutPtr();
            _layout->generation = new NArray<TAllocator, byte>(_allocator, dependencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => default(TSparseBoolConst).Is;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst
        {
            if (default(TSparseBoolConst).Is)
            {
                _layout->generation.Resize(capacity);
                _temp.Get().Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
        {
            _layout->generation.Resize(capacity);
            _temp.Get().Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _layout->generation.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateGeneration(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id)
        {
            unchecked
            {
                ++_layout->generation.GetRef(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage1(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint denseAllocated)
        {
            UnsafeMemory.MemCopy(_temp.ReadPtr(), _layout->generation.ReadPtr(), denseAllocated * sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, byte> GetTempGeneration()
            => _temp.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<TAllocator, byte> GetGeneration()
            => _layout->generation;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_allocator->GetId());
            _layoutMemoryHandle.Pack(ref writer);

            SerializeActions.PackStorageBlittable(ref writer, _layout);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            var allocatorId = reader.ReadUInt32();
            _layoutMemoryHandle.Unpack(ref reader);
            reader.Dependency.Get<WPtr<TAllocator>>(allocatorId).Value->Repair(ref _layoutMemoryHandle);
            _layout = GetLayoutPtr();

            SerializeActions.UnpackStorageBlittable(ref reader, _layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GenerationULayout<TAllocator>* GetLayoutPtr()
            => (GenerationULayout<TAllocator>*)_layoutMemoryHandle.GetPtr();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            repairMemoryContext.Repair(_allocator->GetId(), ref _layoutMemoryHandle);
            _layout = GetLayoutPtr();
        }
    }
}
