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
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>
        {
            var data = new AttachIterator() { data = new AttachData() { context = new() { _state = state } } };
            sparseProvider.ForEach(ref layout, ref data, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>
        {
            var data = new VersionAttachIterator() { data = new VersionAttachData() { context = new() { _state = state }, generation = generation, newGeneration = newGeneration } };
            sparseProvider.ForEach(ref layout, ref data, startIndex, count);
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

        private struct VersionAttachIterator : IDataIterator<TDense>
        {
            public VersionAttachData data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(uint index, ref TDense component)
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

        private struct AttachIterator : IDataIterator<TDense>
        {
            public AttachData data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(uint index, ref TDense component)
            {
                component.OnAttach(ref data.context);
            }
        }
    }
}
