using System.Runtime.CompilerServices;
using Unity.Collections;

namespace AnotherECS.Unity.Jobs
{
    public unsafe struct JobCollectionBag<T0> : IJobBag
      where T0 : unmanaged

    {
        internal int count;
        [NativeDisableParallelForRestriction] internal NativeArray<T0> data;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public T0 Read(int index)
            => data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T0 Get(int index)
            => ref data.GetRef(index);

        void IJobBag.BeginForEachIndex(int chunkIndex) { }
        void IJobBag.EndForEachIndex() { }
    }
}