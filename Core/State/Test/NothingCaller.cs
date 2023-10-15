using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Caller
{
    [IgnoreCompile]
    internal unsafe struct Nothing : IData, IBoolConst
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, ref GlobalDepencies depencies) { }
    }

    [IgnoreCompile]
    internal unsafe struct Nothing<TSparse, TDense, TDenseIndex, TTickData> :
        IData,
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IInject<TSparse, TDense, TDenseIndex, TTickData>,
        IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TDense>,
        IBoolConst,
        IDefaultSetter<TDense>,
        IAttachDetachProvider<TSparse>,
        IAttach<TSparse, TDense, TDenseIndex, TTickData>,
        IDetach<TSparse, TDense, TDenseIndex, TTickData>,
        IChange<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseCopyable<TDense>,
        IVersion<TSparse, TDense, TDenseIndex, TTickData>,
        IHistory<TSparse, TDense, TDenseIndex, TTickData, TDense>,
        ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TDense>
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TDense>
            => default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TDense>
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex index) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(ref TDense component)
        {
            component = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle<TSparse1, TDenseIndex1, TTickData1, TTickDataDense, TSparseStorage>(ref UnmanagedLayout<TSparse1, TDense, TDenseIndex1, TTickData1> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TSparse1 : unmanaged
            where TDenseIndex1 : unmanaged
            where TTickData1 : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
            where TSparseStorage : struct, IIterator<TSparse1, TDense, TDenseIndex1, TTickData1>
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ref TDense source, ref TDense destination)
        {
            destination = source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint id)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryRecycle(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayPtr<TSparse> GetSparseTempBuffer()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayPtr<Op> GetOps()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout) { }
    }
}