using System;

namespace AnotherECS.Core.Threading
{
    public interface IThreadScheduler<TTask> : IDisposable
        where TTask : struct, ITask
    {
        int ParallelMax { get; set; }

        void Run(TTask task);
        void Run(Span<TTask> task);

        void CallFromMainThread();

        bool IsBusy();
        int GetInWork();
        void Wait();
        void Complete();
    }
}