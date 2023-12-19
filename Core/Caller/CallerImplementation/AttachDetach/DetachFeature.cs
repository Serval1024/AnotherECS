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
        public void Detach<TSparseProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            default(TSparseProvider).ForEach<DetachIterable, DetachData>(ref layout, new DetachData() { state = state }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach<TSparseProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> version, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            default(TSparseProvider).ForEach<VersionDetachIterable, VersionDetachData>(
                ref layout,
                new VersionDetachData() { state = state, version = version, newVersion = layout.storage.addRemoveVersion },
                startIndex,
                count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(State state, ref TDense component)
        {
            component.OnDetach(state);
        }


        private struct VersionDetachData : IEachData
        {
            public State state;
            public NArray<BAllocator, byte> version;
            public NArray<TAllocator, byte> newVersion;
        }

        private struct VersionDetachIterable : IDataIterable<TAllocator, TSparse, TDense, TDenseIndex, VersionDetachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref VersionDetachData data, uint index, ref TDense component)
            {
                if (data.version.Read(index) != data.newVersion.Read(index))
                {
                    component.OnDetach(data.state);
                }
            }
        }

        private struct DetachData : IEachData
        {
            public State state;
        }

        private struct DetachIterable : IDataIterable<TAllocator, TSparse, TDense, TDenseIndex, DetachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref DetachData data, uint index, ref TDense component)
            {
                component.OnDetach(data.state);
            }
        }
    }
}
