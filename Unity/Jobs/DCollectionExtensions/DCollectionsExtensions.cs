using AnotherECS.Collections;
using AnotherECS.Core;
using Unity.Jobs;

namespace AnotherECS.Unity.Jobs
{
    public unsafe static class DCollectionsExtensions
    {
        public unsafe static class BagJobFactory
        {
            public static JobCollectionBagR<T0> CreateR<T0>(DArray<T0> data, State state)
                where T0 : unmanaged
            {
                var arrayProvider = JobsGlobalRegister.Get(state);

                JobCollectionBagR<T0> bag;
                bag.data = arrayProvider.GetNativeArrayById(data.Id, data.ToWArray());
                bag.count = (int)data.Length;
                return bag;
            }

            public static JobCollectionBag<T0> Create<T0>(DArray<T0> data, State state)
                where T0 : unmanaged
            {
                var arrayProvider = JobsGlobalRegister.Get(state);

                JobCollectionBag<T0> bag;
                bag.data = arrayProvider.GetNativeArrayById(data.Id, data.ToWArray());
                bag.count = (int)data.Length;
                return bag;
            }

            public static JobCollectionBagR<T0> CreateR<T0>(DList<T0> data, State state)
               where T0 : unmanaged
            {
                var arrayProvider = JobsGlobalRegister.Get(state);

                JobCollectionBagR<T0> bag;
                bag.data = arrayProvider.GetNativeArrayById(data.Id, data.ToWArray());
                bag.count = (int)data.Count;
                return bag;
            }

            public static JobCollectionBag<T0> Create<T0>(DList<T0> data, State state)
                where T0 : unmanaged
            {
                var arrayProvider = JobsGlobalRegister.Get(state);

                JobCollectionBag<T0> bag;
                bag.data = arrayProvider.GetNativeArrayById(data.Id, data.ToWArray());
                bag.count = (int)data.Count;
                return bag;
            }
        }

        public static JobHandle AsJobParallelR<TJob, T0>(this DArray<T0> data, State state, TJob job = default)
            where TJob : struct, IJobParallelForBag<JobCollectionBagR<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.CreateR(data, state));

        public static JobHandle AsJobR<TJob, T0>(this DArray<T0> data, State state, TJob job = default)
            where TJob : struct, IJobForBag<JobCollectionBagR<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.CreateR(data, state));

        public static JobHandle AsJobParallel<TJob, T0>(this DArray<T0> data, State state, TJob job = default)
            where TJob : struct, IJobParallelForBag<JobCollectionBag<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.Create(data, state));

        public static JobHandle AsJob<TJob, T0>(this DArray<T0> data, State state, TJob job = default)
            where TJob : struct, IJobForBag<JobCollectionBag<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.Create(data, state));


        public static JobHandle AsJobParallelR<TJob, T0>(this DList<T0> data, State state, TJob job = default)
            where TJob : struct, IJobParallelForBag<JobCollectionBagR<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.CreateR(data, state));

        public static JobHandle AsJobR<TJob, T0>(this DList<T0> data, State state, TJob job = default)
            where TJob : struct, IJobForBag<JobCollectionBagR<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.CreateR(data, state));

        public static JobHandle AsJobParallel<TJob, T0>(this DList<T0> data, State state, TJob job = default)
            where TJob : struct, IJobParallelForBag<JobCollectionBag<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.Create(data, state));

        public static JobHandle AsJob<TJob, T0>(this DList<T0> data, State state, TJob job = default)
            where TJob : struct, IJobForBag<JobCollectionBag<T0>>
            where T0 : unmanaged
                => job.Schedule(BagJobFactory.Create(data, state));
    }
}