using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct AttachFeature<TAllocator, TSparse, TDense, TDenseIndex> : IAttach<TAllocator, TSparse, TDense, TDenseIndex>, IBoolConst
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged, IAttach
        where TDenseIndex : unmanaged
    {
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<AttachIterable, AttachData>(ref layout, new AttachData() { state = state }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>
            (ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> generation, NArray<TAllocator, byte> newGeneration, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            sparseProvider.ForEach<VersionAttachIterable, VersionAttachData>(
                ref layout,
                new VersionAttachData() { state = state, generation = generation, newGeneration = newGeneration },
                startIndex,
                count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component)
        {
            component.OnAttach(state);
        }


        private struct VersionAttachData
        {
            public State state;
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
                    component.OnAttach(data.state);
                }
            }
        }
        private struct AttachData
        {
            public State state;
        }

        private struct AttachIterable : IDataIterable<TDense, AttachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref AttachData data, uint index, ref TDense component)
            {
                component.OnAttach(data.state);
            }
        }
    }
}
