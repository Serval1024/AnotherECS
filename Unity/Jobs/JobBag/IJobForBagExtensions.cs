using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace AnotherECS.Unity.Jobs
{
    [JobProducerType(typeof(IJobForBagExtensions.JobProcess<,>))]
    public interface IJobForBag<T>
        where T : struct, IJobBag
    {
        void Execute(ref T bag);
    }

    public static class IJobForBagExtensions
    {    
        public static JobHandle Schedule<T, TBag>(this T jobData, TBag bag, JobHandle inputDeps = default)
            where T : struct, IJobForBag<TBag>
            where TBag : struct, IJobBag
            => ScheduleJob(jobData, bag, inputDeps);

        internal static unsafe JobHandle ScheduleJob<T, TBag>(T jobData, TBag bag, JobHandle inputDeps)
            where T : struct, IJobForBag<TBag>
            where TBag : struct, IJobBag
            => new JobProcess<T, TBag>()
            {
                jobData = new JobData<T, TBag>
                {
                    jobData = jobData,
                    bag = bag,
                }
            }.Schedule(inputDeps);

        internal struct JobProcess<T, TBag> : IJob
            where T : struct, IJobForBag<TBag>
            where TBag : struct, IJobBag
        {
            public JobData<T, TBag> jobData;

            public void Execute()
            {
                jobData.bag.BeginForEachIndex(0);
                jobData.jobData.Execute(ref jobData.bag);
                jobData.bag.EndForEachIndex();
            }
        }

        internal struct JobData<T, TBag>
            where T : struct
            where TBag : struct, IJobBag
        {
            public T jobData;
            public TBag bag;
        }
    }
}