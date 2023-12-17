using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Caller
{
    [IgnoreCompile]
    internal unsafe struct Nothing : IData, IBoolConst, IBinderToFilters
    {
        public bool IsTemporary { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, GlobalDepencies* depencies) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config(ref GlobalDepencies depencies) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref GlobalDepencies depencies, uint id, ushort elementId) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref GlobalDepencies depencies, uint id, ushort elementId) { }
    }

    [IgnoreCompile]
    internal unsafe struct Nothing<TAllocator, TSparse, TDense, TDenseIndex> :
        IData,
        ILayoutAllocator<TAllocator, TSparse, TDense, TDenseIndex>,
        ISparseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IInject<TAllocator, TSparse, TDense, TDenseIndex>,
        IBoolConst,
        IDefaultSetter<TDense>,
        IAttachDetachProvider<TSparse>,
        IAttach<TAllocator, TSparse, TDense, TDenseIndex>,
        IDetach<TAllocator, TSparse, TDense, TDenseIndex>,
        IChange<TAllocator, TSparse, TDense, TDenseIndex>,
        IVersion<TAllocator, TSparse, TDense, TDenseIndex>,
        ICustomSerialize<TAllocator, TSparse, TDense, TDenseIndex>,
        IDenseResize<TAllocator, TSparse, TDense, TDenseIndex>,
        IIterator<TAllocator, TSparse, TDense, TDenseIndex>,
        IRevertFinished,
        IRebindMemory<TAllocator, TSparse, TDense, TDenseIndex>

        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
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
        public void Allocate(State state, GlobalDepencies* depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, TAllocator* allocator, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, TDenseIndex index) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropChange(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, uint startIndex, uint count) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint id)
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, TSparse> GetSparseTempBuffer()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, Op> GetOps()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout, State state, ref NArray<BAllocator, Op> ops)
            where JSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<TAllocator, TSparse, TDense, TDenseIndex> { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebindMemory(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref MemoryRebinderContext rebinder, ref TDense component) { }
    }
}