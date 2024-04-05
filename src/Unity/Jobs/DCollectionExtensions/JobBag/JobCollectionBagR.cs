using System.Runtime.CompilerServices;
using Unity.Collections;

namespace AnotherECS.Unity.Jobs
{
    public unsafe struct JobCollectionBagR<T0> : IJobBag
       where T0 : unmanaged

    {
        internal int count;
        [NativeDisableParallelForRestriction][ReadOnly] internal NativeArray<T0> data;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public T0 ReadT0(int index)
            => data[index];

        void IJobBag.BeginForEachIndex(int chunkIndex) { }
        void IJobBag.EndForEachIndex() { }
    }
}