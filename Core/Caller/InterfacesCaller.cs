using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe interface IAllocatorProvider<TStage0Allocator, TStage1Allocator>
        where TStage0Allocator : unmanaged, IAllocator
        where TStage1Allocator : unmanaged, IAllocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TStage0Allocator* GetStage0(Dependencies* dependencies);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TStage1Allocator* GetStage1(Dependencies* dependencies);
    }

    internal unsafe interface IData<TAllocator>
        where TAllocator : unmanaged, IAllocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Config<TMemoryAllocatorProvider>(Dependencies* dependencies, State state, uint callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>;
    }

    internal interface IStateProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        State GetState();
    }

    internal interface IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref Dependencies dependencies, uint id, uint elementId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref Dependencies dependencies, uint id, uint elementId);
    }

    internal interface IAttachDetach<TAllocator, TSparse, TDense, TDenseIndex> : IStateProvider
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        NArray<BAllocator, byte> GetTempGeneration();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<TAllocator, byte> GetGeneration();          

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RevertStage1(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint denseAllocated);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateGeneration(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id);
    }

    internal interface IStartIndexProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetIndex();
    }

    internal interface ISingleDenseFlag
    {
        bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }

    internal interface IDenseProvider<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TDense ReadDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TDense GetDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCapacity(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetAllocated(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> ReadDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<TDense> GetDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);
    }

    internal interface IUseSparse
    {
        public bool IsUseSparse { get; }
    }

    internal interface ISparseProvider<TAllocator, TSparse, TDense, TDenseIndex> : IUseSparse, IData<TAllocator>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsHas(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex ConvertToDenseIndex(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TSparse ReadSparse(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        WArray<T> ReadSparse<T>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies)
            where T : unmanaged;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TSparse GetSparse(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSparse(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id, TDenseIndex denseIndex);
    }

    internal unsafe interface ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LayoutAllocate(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref Dependencies dependencies);
    }

    internal interface IInject<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Construct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Deconstruct(ref ComponentFunction<TDense> componentFunction, ref Dependencies dependencies, ref TDense component);
    }

    internal interface ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsSparseResize<TSparseBoolConst>()
            where TSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst;
    }

    internal interface IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DenseResize(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity);
    }

    internal interface IIdAllocator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex AllocateId<TNumberProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies)
            where TNumberProvider : struct, INumberProvier<TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DeallocateId(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, TDenseIndex id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCount(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex);
    }

    internal interface IChange<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Change(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, TDenseIndex index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DropChange(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, uint startIndex, uint count);
    }

    internal interface INumberProvier<TDenseIndex>
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex Next(uint number);
        public uint ToNumber(TDenseIndex number);
        public TDenseIndex ToGeneric(uint number);
    }

    internal interface IRevertFinished
    {
        bool IsRevertFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }
   
    internal interface IVersion<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetVersion(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        WArray<uint> ReadVersion(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);
    }

    internal interface IBoolConst
    {
        bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }

    internal interface IDefaultSetter<TDense>
    {
        void SetupDefault(ref TDense component);
    }

    internal unsafe interface IAttachExternal<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach(State state, ref TDense component);
    }

    internal unsafe interface IDetachExternal<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach(State state, ref TDense component);
    }

    internal unsafe interface IIterable<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        void ForEach<IIterator>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where IIterator : struct, IIterator<TAllocator, TSparse, TDense, TDenseIndex>;
    }

    internal unsafe interface IIterator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, ref TDense component);
    }

    internal unsafe interface IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>
      where TAllocator : unmanaged, IAllocator
      where TSparse : unmanaged
      where TDense : unmanaged
      where TDenseIndex : unmanaged
    {
        void ForEach<TEachData>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TEachData data, uint startIndex, uint count)
            where TEachData : struct, IDataIterator<TDense>;
    }

    internal unsafe interface IDataIterator<TDense>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(uint index, ref TDense component);
    }

    internal unsafe interface ICallerSerialize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Pack(ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Unpack(ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout);
    }

    internal interface ITickFinished<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TickFinished
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies);
    }

    internal interface IRepairMemory<TDense>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RepairMemory(ref ComponentFunction<TDense> componentFunction, ref RepairMemoryContext repairMemoryContext, ref TDense component);
    }

    internal interface IRepairStateId<TDense>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RepairStateId(ref ComponentFunction<TDense> componentFunction, ushort stateId, ref TDense component);
    }
}
