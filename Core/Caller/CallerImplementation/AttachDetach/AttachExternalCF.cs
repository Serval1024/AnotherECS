using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachExternalCF<TAllocator, TSparse, TDense, TDenseIndex> : IAttachExternal<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IAttachExternal
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<AttachIterable, AttachData>(ref layout, new AttachData() { context = new() { _state = state } }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<VersionAttachIterable, VersionAttachData>(
                ref layout,
                new VersionAttachData() { context = new() { _state = state }, generation = generation, newGeneration = newGeneration },
                startIndex,
                count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component)
        {
            var context = new ADExternalContext() { _state = state, };
            component.OnAttach(ref context);
        }

        private struct VersionAttachData
        {
            public ADExternalContext context;
            public NArray<BAllocator, byte> generation;
            public NArray<TAllocator, byte> newGeneration;
        }

        private struct VersionAttachIterable : IDataIterable<TDense, VersionAttachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref VersionAttachData data, uint index, ref TDense component)
            {
                if (data.generation.Read(index) != data.newGeneration.Read(index))
                {
                    component.OnAttach(ref data.context);
                }
            }
        }
        private struct AttachData
        {
            public ADExternalContext context;
        }

        private struct AttachIterable : IDataIterable<TDense, AttachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref AttachData data, uint index, ref TDense component)
            {
                component.OnAttach(ref data.context);
            }
        }
    }
}
