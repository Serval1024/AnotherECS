using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
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

    internal unsafe interface IData : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Config(State state, Dependencies* dependencies);
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
        public void Add(ref Dependencies dependencies, uint id, ushort elementId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref Dependencies dependencies, uint id, ushort elementId);
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
        ref TDense GetDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TDense ReadDense(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCapacity(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetAllocated(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> GetDense<T>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where T : unmanaged, IComponent;
    }

    internal interface IUseSparse
    {
        public bool IsUseSparse { get; }
    }

    internal unsafe interface IExternalFromCallerConfig
    {
        void Config(Dependencies* dependencies, ushort callerId);
    }

    internal interface ISparseProvider<TAllocator, TSparse, TDense, TDenseIndex> : IUseSparse, IExternalFromCallerConfig
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
        ref TSparse GetSparse(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSparse(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, EntityId id, TDenseIndex denseIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        WArray<T> ReadSparse<T>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where T : unmanaged;
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

    internal unsafe interface IAttach<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach(State state, ref TDense component);
    }

    internal unsafe interface IDetach<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach(State state, ref TDense component);
    }

    internal unsafe interface IIterator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        void ForEach<AIterable>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, TSparse, TDense, TDenseIndex>;
    }

    internal unsafe interface IIterable<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref Dependencies dependencies, ref TDense component);
    }

    internal unsafe interface IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
      where TAllocator : unmanaged, IAllocator
      where TSparse : unmanaged
      where TDense : unmanaged
      where TDenseIndex : unmanaged
    {
        void ForEach<AIterable, TEachData>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TEachData data, uint startIndex, uint count)
            where AIterable : struct, IDataIterable<TDense, TEachData>
            where TEachData : struct, IEachData;
    }

    internal unsafe interface IDataIterable<TDense, TEachData>
        where TDense : unmanaged
        where TEachData : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(ref TEachData data, uint index, ref TDense component);
    }

    internal unsafe interface IEachData { }

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

    internal interface IRebindMemory { }

    internal interface IRebindMemory<TAllocator, TSparse, TDense, TDenseIndex> : IRebindMemory
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RebindMemory(ref ComponentFunction<TDense> componentFunction, ref MemoryRebinderContext rebinder, ref TDense component);
    }
}
