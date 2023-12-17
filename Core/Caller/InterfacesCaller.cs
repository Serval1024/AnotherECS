using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe interface IAllocaterProvider<TAllocator>
        where TAllocator : unmanaged, IAllocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TAllocator* Get(GlobalDepencies* depencies);
    }

    internal unsafe interface IData : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Allocate(State state, GlobalDepencies* depencies);
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
        public void Add(ref GlobalDepencies depencies, uint id, ushort elementId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDepencies depencies, uint id, ushort elementId);
    }

    internal interface IAttachDetachProvider<TSparse> : IStateProvider
        where TSparse : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        NArray<BAllocator, TSparse> GetSparseTempBuffer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        NArray<BAllocator, Op> GetOps();
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
        ref TDense GetDense(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe TDense* GetDensePtr(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCapacity(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetAllocated(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout);
    }

    internal interface IUseSparse
    {
        public bool IsUseSparse { get; }
    }

    internal unsafe interface IExternalFromCallerConfig
    {
        void Config(GlobalDepencies* depencies, ushort callerId);
    }

    internal interface ISparseProvider<TAllocator, TSparse, TDense, TDenseIndex> : IUseSparse, IExternalFromCallerConfig
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsHas(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex ConvertToDenseIndex(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TSparse GetSparse(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSparse(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, EntityId id, TDenseIndex denseIndex);
    }

    internal unsafe interface ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDepencies depencies);
    }

    internal interface IInject<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Construct(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Deconstruct(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component);
    }

    internal interface ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst;
    }

    internal interface IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity);
    }

    internal interface IIdAllocator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex AllocateId<TNumberProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies)
            where TNumberProvider : struct, INumberProvier<TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DeallocateId(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, TDenseIndex id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCount(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex);
    }

    internal interface IChange<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Change(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, TDenseIndex index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DropChange(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, uint startIndex, uint count);
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
    /*
    internal interface IDenseRevert<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RevertDense(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint tick);
    }*/
    /*
    internal interface IDenseCopyable<TDense>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CopyFrom(ref TDense source, ref TDense destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Recycle(ref TDense component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Recycle<TAllocator, TSparse, TDenseIndex, TSparseStorage>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDenseIndex : unmanaged
            where TSparseStorage : struct, IIterator<TAllocator, TSparse, TDense, TDenseIndex>;
    }
    */
    internal interface IVersion<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, EntityId id);
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
        void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst;

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
        void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach(State state, ref TDense component);
    }

    internal unsafe interface IIterator<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, TSparse, TDense, TDenseIndex>;
    }

    internal unsafe interface IIterable<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component);
    }

    internal unsafe interface ICustomSerialize<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout);
    }

    internal interface ITickFinished<TAllocator, TSparse, TDense, TDenseIndex>
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TickFinished
            (ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies);
    }

    internal interface IRebindMemory { }

    internal interface IRebindMemory<TAllocator, TSparse, TDense, TDenseIndex> : IRebindMemory
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RebindMemory(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref MemoryRebinderContext rebinder, ref TDense component);
    }

    /*
    internal unsafe interface IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushRecycledCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint recycleIndex)
        {
            HistoryActions.PushRecycledCount<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint count)
        {
            HistoryActions.PushCount<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushSparse(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, EntityId id, TSparse sparse)
        {
            HistoryActions.PushSparse<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, id, sparse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushDense<TCopyable, TUintNextNumber>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
            where TUintNextNumber : struct, INumberProvier<TDenseIndex>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushRecycled(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint offset, TDenseIndex recycle)
        {
            HistoryActions.PushRecycled<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, offset, recycle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse;
    }

    internal unsafe interface ISegmentHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense> : IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushDenseSegment
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TTickDataDense* data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushSegmentDense
            (ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint offset, uint index, TTickDataDense* data);
    }
    */
}