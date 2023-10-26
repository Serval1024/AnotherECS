using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Caller
{
    [IgnoreCompile]
    internal unsafe struct Nothing : IData, IBoolConst, ITickData<Nothing>
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public uint Tick => throw new System.NotImplementedException();
        public Nothing Value => throw new System.NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, ref GlobalDepencies depencies) { }
    }

    [IgnoreCompile]
    internal unsafe struct Nothing<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense> :
        IData,
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IInject<TSparse, TDense, TDenseIndex, TTickData>,
        IBoolConst,
        IDefaultSetter<TDense>,
        IAttachDetachProvider<TSparse>,
        IAttach<TSparse, TDense, TDenseIndex, TTickData>,
        IDetach<TSparse, TDense, TDenseIndex, TTickData>,
        IChange<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseCopyable<TDense>,
        IVersion<TSparse, TDense, TDenseIndex, TTickData>,
        IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>,
        ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IIterator<TSparse, TDense, TDenseIndex, TTickData>,
        ISegmentHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>,
        IRevertFinished

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsRevertFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<JSparseBoolConst>()
            where JSparseBoolConst : struct, IBoolConst
            => false; 

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
        public void Change(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex index) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropChange(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(ref TDense component)
        {
            component = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle<TSparse1, TDenseIndex1, TTickData1, TTickDataDense1, TSparseStorage>(ref UnmanagedLayout<TSparse1, TDense, TDenseIndex1, TTickData1> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TSparse1 : unmanaged
            where TDenseIndex1 : unmanaged
            where TTickData1 : unmanaged, ITickData<TTickDataDense1>
            where TTickDataDense1 : unmanaged
            where TSparseStorage : struct, IIterator<TSparse1, TDense, TDenseIndex1, TTickData1> { }
       
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
        public void PushDense<TCopyable, TUintNextNumber>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
            where TUintNextNumber : struct, INumberProvier<TDenseIndex> { }

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
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TSparse, TDense, TDenseIndex, TTickData> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout) { }

        public void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, TIsUseSparse>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
            where TIsUseSparse : struct, IUseSparse { }

        public void PushSegmentDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TTickDataDense* data) { }
    }
}