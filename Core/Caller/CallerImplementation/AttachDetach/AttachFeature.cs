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
        public void Attach<TSparseProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, State state, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            default(TSparseProvider).ForEach<AttachIterable, AttachData>(ref layout, new AttachData() { state = state }, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach<TSparseProvider>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, ref TSparseProvider sparseProvider, State state, NArray<BAllocator, byte> version, uint startIndex, uint count)
            where TSparseProvider : struct, IDataIterator<TAllocator, TSparse, TDense, TDenseIndex>
        {
            default(TSparseProvider).ForEach<VersionAttachIterable, VersionAttachData>(
                ref layout,
                new VersionAttachData() { state = state, version = version, newVersion = layout.storage.addRemoveVersion },
                startIndex,
                count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(State state, ref TDense component)
        {
            component.OnAttach(state);
        }


        private struct VersionAttachData : IEachData
        {
            public State state;
            public NArray<BAllocator, byte> version;
            public NArray<TAllocator, byte> newVersion;
        }

        private struct VersionAttachIterable : IDataIterable<TAllocator, TSparse, TDense, TDenseIndex, VersionAttachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref VersionAttachData data, uint index, ref TDense component)
            {
                if (data.version.Read(index) != data.newVersion.Read(index))
                {
                    component.OnAttach(data.state);
                }
            }
        }
        private struct AttachData : IEachData
        {
            public State state;
        }

        private struct AttachIterable : IDataIterable<TAllocator, TSparse, TDense, TDenseIndex, AttachData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Each(ref AttachData data, uint index, ref TDense component)
            {
                component.OnAttach(data.state);
            }
        }
    }
}
