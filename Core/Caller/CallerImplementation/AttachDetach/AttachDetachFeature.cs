﻿using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachDetachFeature<TAllocator, TSparse, TDense, TDenseIndex> :
        IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex>,
        IData,
        IBoolConst,
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        ISerialize

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
        public void Config(State state, GlobalDependencies* dependencies)
        {
            this.state = state;
            _temp = new NContainer<BAllocator, NArray<BAllocator, byte>>(&dependencies->bAllocator, default);
            _temp.GetRef() = new NArray<BAllocator, byte>(&dependencies->bAllocator, dependencies->config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDependencies dependencies)
        {
            _allocator = allocator;
            _layoutMemoryHandle = allocator->Allocate((uint)sizeof(GenerationULayout<TAllocator>));
            _layout = GetLayoutPtr();
            _layout->generation = new NArray<TAllocator, byte>(allocator, dependencies.config.general.componentCapacity);
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
            reader.GetDepency<WPtr<TAllocator>>(allocatorId).Value->Repair(ref _layoutMemoryHandle);
            _layout = GetLayoutPtr();

            SerializeActions.UnpackStorageBlittable(ref reader, _layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GenerationULayout<TAllocator>* GetLayoutPtr()
            => (GenerationULayout<TAllocator>*)_layoutMemoryHandle.GetPtr();
    }
}
