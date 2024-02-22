using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace AnotherECS.Unity.Jobs
{
    [JobProducerType(typeof(IJobForBagParallelForExtensions.JobProcess<,>))]
    public interface IJobParallelForBag<T>
        where T : struct, IJobBag
    {
        void Execute(ref T bag, int index);
    }

    public static class IJobForBagParallelForExtensions
    {    
        public static JobHandle Schedule<T, TBag>(this T jobData, TBag bag, JobHandle inputDeps = default)
            where T : struct, IJobParallelForBag<TBag>
            where TBag : struct, IJobBag
            => ScheduleJob(jobData, bag, inputDeps);

        internal static unsafe JobHandle ScheduleJob<T, TBag>(T jobData, TBag bag, JobHandle inputDeps)
            where T : struct, IJobParallelForBag<TBag>
            where TBag : struct, IJobBag
        {
            
            var data = new JobData<T, TBag>
            {
                jobData = jobData,
                bag = bag,
            };

            var parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref data), JobProcess<T, TBag>.Initialize(), inputDeps, ScheduleMode.Parallel);
            return JobsUtility.ScheduleParallelFor(ref parameters, bag.Count, JobBagUtils.GetScheduleBatchCount(bag.Count));
        }

        internal struct JobData<T, TBag> where T : struct where TBag : struct, IJobBag
        {
            public T jobData;
            public TBag bag;
        }

        internal struct JobProcess<T, TBag> where T : struct, IJobParallelForBag<TBag>
            where TBag : struct, IJobBag
        {
            private static System.IntPtr jobReflectionData;

            public static System.IntPtr Initialize()
            {
                if (jobReflectionData == System.IntPtr.Zero)
                {
                    jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobData<T, TBag>), typeof(T), (ExecuteJobFunction)Execute);
                }
                return jobReflectionData;
            }

            public delegate void ExecuteJobFunction(ref JobData<T, TBag> jobData, System.IntPtr additionalData, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            public static void Execute(ref JobData<T, TBag> jobData, System.IntPtr additionalData, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out var begin, out var end) == true)
                {    
                    for (int i = begin; i < end; ++i)
                    {
                        jobData.bag.BeginForEachIndex(i);
                        jobData.jobData.Execute(ref jobData.bag, i);
                        jobData.bag.EndForEachIndex();
                    }
                    
                }
            }
        }
    }

}