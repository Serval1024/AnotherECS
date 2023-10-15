using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal interface IData : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Allocate(State state, ref GlobalDepencies depencies);
    }

    internal interface IStateProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        State GetState();
    }

    internal interface IAttachDetachProvider<TSparse> : IStateProvider
        where TSparse : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ArrayPtr<TSparse> GetSparseTempBuffer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ArrayPtr<Op> GetOps();
    }

    internal interface IStartIndexProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetIndex();
    }

    internal interface IDenseProvider<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, TDenseIndex index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout);
    }

    internal interface ISparseProvider<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsHas(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex ConvertToDenseIndex(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref TSparse GetSparse(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, EntityId id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSparse<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, EntityId id, TDenseIndex denseIndex)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>;
    }

    internal interface ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies);
    }

    internal interface IInject<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Construct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Deconstruct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component);
    }

    internal interface ISparseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst;
    }

    internal interface IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity);
    }

    internal interface IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TDenseIndex AllocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex);
    }

    internal interface IChange<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Change(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex index);
    }

    internal interface IDenseRevert<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick);
    }

    internal interface IDenseCopyable<TDense>
        where TDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CopyFrom(ref TDense source, ref TDense destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Recycle(ref TDense component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Recycle<TSparse, TDenseIndex, TTickData, TTickDataDense, TSparseStorage>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TSparse : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TSparseStorage : struct, IIterator<TSparse, TDense, TDenseIndex, TTickData>
            where TTickDataDense : unmanaged;
    }

    internal interface IVersion<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetVersion(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, EntityId id);
    }

    internal interface IBoolConst
    {
        bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }

    internal interface IDefaultSetter<TDense>
    {
        void SetupDefault(ref TDense component);
    }

    internal unsafe interface IAttach<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Attach(State state, ref TDense component);
    }

    internal unsafe interface IDetach<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Detach(State state, ref TDense component);
    }

    internal unsafe interface IIterator<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        void ForEach<AIterable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TSparse, TDense, TDenseIndex, TTickData>;
    }

    internal unsafe interface IIterable<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component);
    }

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
        void PushDense<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst;

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
        void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst;
    }

    internal unsafe interface ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout);
    }
}
