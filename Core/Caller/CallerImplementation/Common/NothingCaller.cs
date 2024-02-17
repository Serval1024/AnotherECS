using AnotherECS.Converter;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    [IgnoreCompile]
    internal unsafe struct Nothing<TAllocator, TSparse, TDense, TDenseIndex> :
        IData<TAllocator>,
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IInject<TAllocator, TSparse, TDense, TDenseIndex>,
        IBoolConst,
        IDefaultSetter<TDense>,
        IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex>,
        IAttachExternal<TAllocator, TSparse, TDense, TDenseIndex>,
        IDetachExternal<TAllocator, TSparse, TDense, TDenseIndex>,
        IChange<TAllocator, TSparse, TDense, TDenseIndex>,
        IVersion<TAllocator, TSparse, TDense, TDenseIndex>,
        ICallerSerialize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IIterable<TAllocator, TSparse, TDense, TDenseIndex>,
        IRevertFinished,
        IRepairMemory<TDense>,
        IRepairStateId<TDense>,
        IRepairMemoryHandle,
        IBinderToFilters,
        ISerialize,
        IDisposable

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsRevertFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst
            => false; 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref Dependencies dependencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, TDenseIndex index) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropChange(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, uint startIndex, uint count) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<uint> ReadVersion(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<IIterator>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where IIterator : struct, IIterator<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, byte> GetTempGeneration() { throw new System.NotSupportedException(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertStage1(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint denseAllocated) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateGeneration(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairMemory(ref ComponentFunction<TDense> componentFunction, ref RepairMemoryContext repairMemoryContext, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<TAllocator, byte> GetGeneration() => default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairStateId(ref ComponentFunction<TDense> componentFunction, ushort stateId, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(Dependencies* dependencies, State state, uint callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref Dependencies dependencies, uint id, uint elementId) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref Dependencies dependencies, uint id, uint elementId) { }
    }
}