using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{

    internal struct DefaultFeature<TDense> : IDefaultSetter<TDense>
        where TDense : struct, IDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component)
        {
            component.Setup();
        }
    }

    internal struct AttachDetachFeature<TSparse> : IData, IAttachDetachProvider<TSparse>, IBoolConst
        where TSparse : unmanaged
    {
        public State state;
        public ArrayPtr<TSparse> bufferCopyTemp;
        public ArrayPtr<Op> opsTemp;

        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(State state, ref GlobalDepencies depencies)
        {
            this.state = state;
            bufferCopyTemp.Allocate(depencies.config.general.entityCapacity);
            opsTemp.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            bufferCopyTemp.Dispose();
            opsTemp.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayPtr<TSparse> GetSparseTempBuffer()
            => bufferCopyTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayPtr<Op> GetOps()
            => opsTemp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public State GetState()
            => state;
    }

    internal unsafe struct AttachFeature<TSparse, TDense, TDenseIndex, TTickData> : IAttach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
            where TSparse : unmanaged
            where TDense : unmanaged, IAttach
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                AttachLayoutActions.Attach_ushort(ref *layoutUshort, state, ref ops);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component)
        {
            component.OnAttach(state);
        }
    }

    internal unsafe struct DetachFeature<TSparse, TDense, TDenseIndex, TTickData> : IDetach<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged, IDetach
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, uint startIndex, uint count)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, startIndex);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_ushort(ref *layoutUshort, state, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<JSparseBoolConst>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout, State state, ref ArrayPtr<Op> ops)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                var layoutBool = (UnmanagedLayout<bool, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_bool(ref *layoutBool, state, ref ops);
            }
            else
            {
                var layoutUshort = (UnmanagedLayout<ushort, TDense, TDenseIndex, TTickData>*)layout;
                DetachLayoutActions.Detach_ushort(ref *layoutUshort, state, ref ops);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component)
        {
            component.OnDetach(state);
        }
    }

    internal struct EmptyFeature<TSparse, TDense, TDenseIndex, TTickData> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IStartIndexProvider
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(1);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }
    }

    internal struct HubLayoutAllocator<TSparse, TDense, TDenseIndex, TTickData, TAllocator0, TAllocator1, TAllocator2, TAllocator3, TAllocator4> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
        where TAllocator0 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator1 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator2 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator3 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
        where TAllocator4 : struct, ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>, ISparseResize<TSparse, TDense, TDenseIndex, TTickData>, IDenseResize<TSparse, TDense, TDenseIndex, TTickData>
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => 
                default(TAllocator0).IsSparseResize ||
                default(TAllocator1).IsSparseResize ||
                default(TAllocator2).IsSparseResize ||
                default(TAllocator3).IsSparseResize ||
                default(TAllocator4).IsSparseResize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.Allocate(ref layout, ref depencies);
            allocator1.Allocate(ref layout, ref depencies);
            allocator2.Allocate(ref layout, ref depencies);
            allocator3.Allocate(ref layout, ref depencies);
            allocator4.Allocate(ref layout, ref depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator1.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator2.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator3.SparseResize<JSparseBoolConst>(ref layout, capacity);
            allocator4.SparseResize<JSparseBoolConst>(ref layout, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
        {
            TAllocator0 allocator0 = default;
            TAllocator1 allocator1 = default;
            TAllocator2 allocator2 = default;
            TAllocator3 allocator3 = default;
            TAllocator4 allocator4 = default;
            allocator0.DenseResize(ref layout, capacity);
            allocator1.DenseResize(ref layout, capacity);
            allocator2.DenseResize(ref layout, capacity);
            allocator3.DenseResize(ref layout, capacity);
            allocator4.DenseResize(ref layout, capacity);
        }
    }

    internal struct UshortDenseFeature<TSparse, TDense, TTickData> :
        ILayoutAllocator<TSparse, TDense, ushort, TTickData>,
        ISparseResize<TSparse, TDense, ushort, TTickData>,
        IDenseResize<TSparse, TDense, ushort, TTickData>,
        IStartIndexProvider,
        IDenseProvider<TSparse, TDense, ushort, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(depencies.config.general.componentCapacity);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.dense.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
        {
            layout.storage.dense.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ushort index)
            => ref layout.storage.dense.GetRef(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout)
            => layout.storage.dense.ElementCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout)
            => layout.storage.denseIndex;
    }

    internal struct TrueConst : IBoolConst
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
    }

    internal interface INextNumber<TSparse>
        where TSparse : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSparse Next(uint number);
    }

    internal struct UshortNextNumber : INextNumber<ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Next(uint number)
            => (ushort)++number;
    }

    internal struct RecycleStorageFeature<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense, RNextNumber> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
        where RNextNumber : unmanaged, INextNumber<TDenseIndex>
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.recycle.Allocate(depencies.config.general.recycledCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => StorageActions.GetCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;

            THistory history = default;
            if (recycleIndex > 0)
            {
                history.PushRecycledCount(ref layout, ref depencies, recycleIndex);
                return storage.recycle.Get(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
#if ANOTHERECS_DEBUG
                StorageActions.CheckDenseLimit<TSparse, TDense, TDenseIndex, TTickData, TSparse>(ref layout);
#endif
                history.PushCount(ref layout, ref depencies, denseIndex);
                RNextNumber nextNumber = default;
                return nextNumber.Next(denseIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        {
            StorageActions.TryResizeDense(ref layout, layout.storage.recycle.ElementCount << 1);
            ref var recycleIndex = ref layout.storage.recycleIndex;
            var recycle = layout.storage.recycle.GetPtr();

            THistory history = default;
            history.PushRecycled(ref layout, ref depencies, recycleIndex, recycle[recycleIndex]);
            history.PushRecycledCount(ref layout, ref depencies, recycleIndex);

            recycle[recycleIndex++] = id;
        }
    }

    internal struct IncrementStorageFeature<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense, RNextNumber> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged

        where TDenseIndex : unmanaged
        where RNextNumber : unmanaged, INextNumber<TDenseIndex>
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => StorageActions.GetSpaceCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        {
            ref var denseIndex = ref layout.storage.denseIndex;
#if ANOTHERECS_DEBUG
            StorageActions.CheckDenseLimit<TSparse, TDense, TDenseIndex, TTickData, TSparse>(ref layout);
#endif
            THistory history = default;
            history.PushCount(ref layout, ref depencies, denseIndex);
            RNextNumber nextNumber = default;
            return nextNumber.Next(denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        { }
    }

    internal struct SingleFeature<TSparse, TDense, TDenseIndex, TTickData> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IStartIndexProvider,
        IDenseProvider<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.dense.Allocate(1);
            storage.denseIndex = GetIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TDense GetDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, TDenseIndex index)
            => ref layout.storage.dense.GetRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            => 0;
    }

    internal struct SingleStorageFeature<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense, RNextNumber> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TTickData>,
        ISparseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IDenseResize<TSparse, TDense, TDenseIndex, TTickData>,
        IIdAllocator<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>

        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where RNextNumber : unmanaged, INextNumber<TDenseIndex>
        where TTickDataDense : unmanaged
    {
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            => StorageActions.GetSpaceCount(ref layout, startIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDenseIndex AllocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        {
            RNextNumber nextNumber = default;
            return nextNumber.Next(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeallocateId<THistory>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, TDenseIndex id)
            where THistory : struct, IHistory<TSparse, TDense, TDenseIndex, TTickData, TTickDataDense>
        { }
    }

    internal struct UshortVersionFeature<TSparse, TDense, TTickData> :
        ILayoutAllocator<TSparse, TDense, ushort, TTickData>,
        ISparseResize<TSparse, TDense, ushort, TTickData>,
        IDenseResize<TSparse, TDense, ushort, TTickData>,
        IChange<TSparse, TDense, ushort, TTickData>,
        IVersion<TSparse, TDense, ushort, TTickData>,
        IBoolConst

        where TSparse : unmanaged
        where TDense : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsSparseResize { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.version.Allocate(depencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.storage.version.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint capacity)
        {
            layout.storage.version.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, ushort index)
        {
            layout.storage.version.Set(index, depencies.tickProvider.tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion(ref UnmanagedLayout<TSparse, TDense, ushort, TTickData> layout, uint id)
            => layout.storage.version.Get(id);
    }

    internal struct ByChangeHistoryFeature<TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>,
        IHistory<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>, TDense>

        where TSparse : unmanaged, IEquatable<TSparse>
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, ref GlobalDepencies depencies)
        {
            ref var history = ref layout.history;
            history.sparseBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.countBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.denseBuffer.Allocate(depencies.config.history.buffersChangeCapacity);

            if (history.recycleBuffer.IsValide)
            {
                history.recycleCountBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
                history.recycleBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.PushDense<TSparse, TDense, TDenseIndex, TCopyable>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryChangeClear<TSparse, TDense, TDenseIndex, TCopyable>(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickOffsetData<TDense>> layout, uint tick)
            {
                HistoryActions.RevertToValueBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
            }
        }
    }

    internal struct ByTickHistoryFeature<TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>,
        IHistory<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>, ArrayPtr<TDense>>

        where TSparse : unmanaged, IEquatable<TSparse>
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, ref GlobalDepencies depencies)
        {
            ref var history = ref layout.history;
            history.sparseBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.countBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.denseBuffer.Allocate(depencies.config.history.buffersChangeCapacity);

            if (history.recycleBuffer.IsValide)
            {
                history.recycleCountBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
                history.recycleBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryTickClear<TSparse, TDense, TDenseIndex, TCopyable>(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.PushFullDense<TSparse, TDense, TDenseIndex, TCopyable>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, layout.storage.dense);
        }

        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion>
            (UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickData<ArrayPtr<TDense>>> layout, uint tick)
            {
                HistoryActions.RevertToValueBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.denseIndex);
            }
        }
    }

    internal struct ByVersionHistoryFeature<TSparse, TDense, TDenseIndex> :
        ILayoutAllocator<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>,
        ISparseResize<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>,
        IDenseResize<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>,
        IHistory<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>, TDense>

        where TSparse : unmanaged, IEquatable<TSparse>
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public bool IsRevert { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }
        public bool IsTickFinished { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, ref GlobalDepencies depencies)
        {
            ref var history = ref layout.history;
            history.sparseBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.countBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            history.denseBuffer.Allocate(depencies.config.history.buffersChangeCapacity);

            if (history.recycleBuffer.IsValide)
            {
                history.recycleCountBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
                history.recycleBuffer.Allocate(depencies.config.history.buffersAddRemoveCapacity);
            }

            history.versionIndexer.Allocate(depencies.config.general.componentCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            JSparseBoolConst sparseBoolConst = default;
            if (sparseBoolConst.Is)
            {
                layout.history.versionIndexer.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, uint capacity)
        {
            layout.history.versionIndexer.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushDense<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, ref GlobalDepencies depencies, TDenseIndex offset, ref TDense data)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HistoryClear<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.HistoryVersionClear<TSparse, TDense, TDenseIndex, TCopyable>(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished<TCopyable>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, ref GlobalDepencies depencies)
            where TCopyable : struct, IDenseCopyable<TDense>, IBoolConst
        {
            HistoryActions.PushVersionDense<TSparse, TDense, TDenseIndex, TCopyable>(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RevertTo<TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion>(UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>* layout, ref TAttachDetachStorage attachDetachStorage, uint tick)
            where TAttachDetachStorage : struct, IAttachDetachProvider<TSparse>, IBoolConst
            where TAttach : struct, IAttach<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>, IBoolConst
            where TDetach : struct, IDetach<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>, IBoolConst
            where JSparseBoolConst : struct, IBoolConst
            where TVersion : struct, IBoolConst
        {
            HistoryActions.RevertTo<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>, TAttachDetachStorage, TAttach, TDetach, JSparseBoolConst, TVersion, RevertDense>
                (layout, ref attachDetachStorage, tick);
        }

        private struct RevertDense : IDenseRevert<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDenseRevert<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>>.RevertDense(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TickIndexerOffsetData<TDense>> layout, uint tick)
            {
                HistoryActions.RevertToValueIndexerBuffer(tick, ref layout.storage.dense, ref layout.history.denseBuffer, ref layout.history.versionIndexer, ref layout.history.denseIndex);
            }
        }
    }

    internal unsafe struct InjectFeature<TSparse, TDense, TDenseIndex, ETickDataDense> : IInject<TSparse, TDense, TDenseIndex, ETickDataDense>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where ETickDataDense : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, ETickDataDense> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            layout.componentFunction.construct(ref depencies.injectContainer, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, ETickDataDense> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            layout.componentFunction.deconstruct(ref depencies.injectContainer, ref component);
        }
    }

    internal unsafe struct ConstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData> : IIterable<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            InjectFeature<TSparse, TDense, TDenseIndex, TTickData> injectFeature = default;
            injectFeature.Construct(ref layout, ref depencies, ref component);
        }
    }

    internal unsafe struct DeconstructInjectIterable<TSparse, TDense, TDenseIndex, TTickData> : IIterable<TSparse, TDense, TDenseIndex, TTickData>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            InjectFeature<TSparse, TDense, TDenseIndex, TTickData> injectFeature = default;
            injectFeature.Deconstruct(ref layout, ref depencies, ref component);
        }
    }

    internal unsafe struct BoolSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<bool, TDense, uint, TTickData>,
        ISparseResize<bool, TDense, uint, TTickData>,
        IDenseResize<bool, TDense, uint, TTickData>,
        ISparseProvider<bool, TDense, uint, TTickData, TTickDataDense>,
        IIterator<bool, TDense, uint, TTickData>,
        IBoolConst

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, uint id)
            => layout.storage.sparse.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<bool, TDense, uint, TTickData>
        {
            AIterable iterable = default;
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr();
            var dense = storage.dense.GetPtr();
            var denseIndex = storage.denseIndex;

            for (uint i = startIndex; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    iterable.Each(ref layout, ref depencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse<THistory>(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, ref GlobalDepencies depencies, uint id, uint denseIndex)
            where THistory : struct, IHistory<bool, TDense, uint, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();

            THistory history = default;
            history.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref UnmanagedLayout<bool, TDense, uint, TTickData> layout, EntityId id)
            => ref layout.storage.sparse.GetRef(id);
    }

    internal unsafe struct UshortSparseFeature<TDense, TTickData, TTickDataDense> :
        ILayoutAllocator<ushort, TDense, ushort, TTickData>,
        ISparseResize<ushort, TDense, ushort, TTickData>,
        IDenseResize<ushort, TDense, ushort, TTickData>,
        ISparseProvider<ushort, TDense, ushort, TTickData, TTickDataDense>,
        IIterator<ushort, TDense, ushort, TTickData>,
        IBoolConst

        where TDense : unmanaged
        where TTickData : unmanaged, ITickData<TTickDataDense>
        where TTickDataDense : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies)
        {
            layout.storage.sparse.Allocate(depencies.config.general.entityCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<JSparseBoolConst>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint capacity)
            where JSparseBoolConst : struct, IBoolConst
        {
            layout.storage.sparse.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ConvertToDenseIndex(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
            => layout.storage.sparse.Get(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
           => layout.storage.sparse.Get(id) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<AIterable>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where AIterable : struct, IIterable<ushort, TDense, ushort, TTickData>
        {
            if (count != 0)
            {
                AIterable iterable = default;

                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr();

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        iterable.Each(ref layout, ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSparse<THistory>(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, ref GlobalDepencies depencies, EntityId id, ushort denseIndex)
            where THistory : struct, IHistory<ushort, TDense, ushort, TTickData, TTickDataDense>
        {
            ref var storage = ref layout.storage;
            var sparse = storage.sparse.GetPtr();
            var dense = storage.dense.GetPtr();

            THistory history = default;
            history.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref ushort GetSparse(ref UnmanagedLayout<ushort, TDense, ushort, TTickData> layout, uint id)
            => ref layout.storage.sparse.GetRef(id);
    }

    internal struct CopyableFeature<TDense> : IDenseCopyable<TDense>, IBoolConst
        where TDense : unmanaged, ICopyable<TDense>
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ref TDense source, ref TDense destination)
        {
            destination.CopyFrom(in source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(ref TDense component)
        {
            component.OnRecycle();
            component = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle<TSparse, TDenseIndex, TTickData, TTickDataDense, TSparseStorage>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, uint startIndex, uint count)
            where TSparse : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ITickData<TTickDataDense>
            where TTickDataDense : unmanaged
            where TSparseStorage : struct, IIterator<TSparse, TDense, TDenseIndex, TTickData>
        {
            TSparseStorage sparseStorage = default;
            sparseStorage.ForEach<RecycleIterable<TSparse, TDense, TDenseIndex, TTickData>>(ref layout, ref depencies, startIndex, count);
        }
    }

    internal unsafe struct RecycleIterable<TSparse, TDense, TDenseIndex, TTickData> : IIterable<TSparse, TDense, TDenseIndex, TTickData>
       where TSparse : unmanaged
       where TDense : unmanaged, ICopyable<TDense>
       where TDenseIndex : unmanaged
       where TTickData : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Each(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, ref GlobalDepencies depencies, ref TDense component)
        {
            CopyableFeature<TDense> �opyableFeature = default;
            �opyableFeature.Recycle(ref component);
        }
    }

    internal unsafe struct BBSerialize<TSparse, TDense, TDenseIndex, TTickData> : ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.PackStorageBlittable(ref writer, layout);
            SerializeActions.PackHistoryBlittable(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.UnpackStorageBlittable(ref reader, layout);
            SerializeActions.UnpackHistoryBlittable(ref reader, layout);
        }
    }

    internal unsafe struct BSSerialize<TSparse, TDense, TDenseIndex, TTickData> : ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ISerialize
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.PackStorageBlittable(ref writer, layout);
            SerializeActions.PackHistorySerialize(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.UnpackStorageBlittable(ref reader, layout);
            SerializeActions.UnpackHistorySerialize(ref reader, layout);
        }
    }

    internal unsafe struct SSSerialize<TSparse, TDense, TDenseIndex, TTickData> : ICustomSerialize<TSparse, TDense, TDenseIndex, TTickData>, IBoolConst
        where TSparse : unmanaged
        where TDense : unmanaged, ISerialize
        where TDenseIndex : unmanaged
        where TTickData : unmanaged, ISerialize
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        public void Pack(ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.PackStorageSerialize(ref writer, layout);
            SerializeActions.PackHistorySerialize(ref writer, layout);
        }

        public void Unpack(ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
        {
            SerializeActions.UnpackStorageSerialize(ref reader, layout);
            SerializeActions.UnpackHistorySerialize(ref reader, layout);
        }
    }
}