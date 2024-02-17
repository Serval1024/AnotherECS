using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DetachExternalCF<TAllocator, TSparse, TDense, TDenseIndex> : IDetachExternal<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IDetachExternal
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>
        {
            var data = new DetachIterator() { data = new DetachData() { context = new() { _state = state } } };
            sparseProvider.ForEach(ref layout, ref data, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterable<TAllocator, TSparse, TDense, TDenseIndex>
        {
            var data = new VersionDetachIterator() { data = new VersionDetachData() { context = new() { _state = state }, generation = generation, newGeneration = newGeneration } };
            sparseProvider.ForEach(ref layout, ref data, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component)
        {
            var context = new ADExternalContext() { _state = state, };
            component.OnDetach(ref context);
        }


        private struct VersionDetachData
        {
            public ADExternalContext context;
            public NArray<BAllocator, byte> generation;
            public NArray<TAllocator, byte> newGeneration;
        }

        private struct VersionDetachIterator : IDataIterator<TDense>
        {
            public VersionDetachData data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(uint index, ref TDense component)
            {
                if (data.generation.Read(index) != data.newGeneration.Read(index))
                {
                    component.OnDetach(ref data.context);
                }
            }
        }

        private struct DetachData
        {
            public ADExternalContext context;
        }

        private struct DetachIterator : IDataIterator<TDense>
        {
            public DetachData data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(uint index, ref TDense component)
            {
                component.OnDetach(ref data.context);
            }
        }
    }
}
