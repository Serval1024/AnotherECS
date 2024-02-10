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
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<DetachIterable, DetachData>(ref layout, new DetachData() { context = new() { _state = state } }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<VersionDetachIterable, VersionDetachData>(
                ref layout,
                new VersionDetachData() { context = new() { _state = state }, generation = generation, newGeneration = newGeneration },
                startIndex,
                count);
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

        private struct VersionDetachIterable : IDataIterable<TDense, VersionDetachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref VersionDetachData data, uint index, ref TDense component)
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

        private struct DetachIterable : IDataIterable<TDense, DetachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref DetachData data, uint index, ref TDense component)
            {
                component.OnDetach(ref data.context);
            }
        }
    }
}
