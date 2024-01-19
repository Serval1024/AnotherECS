using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DetachFeature<TAllocator, TSparse, TDense, TDenseIndex> : IDetach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IDetach
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<DetachIterable, DetachData>(ref layout, new DetachData() { state = state }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<VersionDetachIterable, VersionDetachData>(
                ref layout,
                new VersionDetachData() { state = state, generation = generation, newGeneration = newGeneration },
                startIndex,
                count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component)
        {
            component.OnDetach(state);
        }


        private struct VersionDetachData
        {
            public State state;
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
                    component.OnDetach(data.state);
                }
            }
        }

        private struct DetachData
        {
            public State state;
        }

        private struct DetachIterable : IDataIterable<TDense, DetachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref DetachData data, uint index, ref TDense component)
            {
                component.OnDetach(data.state);
            }
        }
    }
}
