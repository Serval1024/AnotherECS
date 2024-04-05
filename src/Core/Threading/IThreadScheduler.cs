using System;

namespace AnotherECS.Core.Threading
{
    internal interface IThreadScheduler<TTask> : IDisposable
        where TTask : struct, ITask
    {
        int ParallelMax { get; set; }

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        ITimerStatistic Statistic { get; set; }
#endif

        void Run(TTask task);
        void Run(Span<TTask> task);

        void CallFromMainThread();

        bool IsBusy();
        int GetInWork();
        void Clear();
        void Wait();
        void Complete();
    }
}