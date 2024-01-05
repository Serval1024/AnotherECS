using System;

namespace AnotherECS.Core.Threading
{
    public interface IThreadScheduler : IDisposable
    {
        int ParallelMax { get; set; }

        void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct;

        void CallFromMainThread();

        bool IsBusy();
    }

    public struct ThreadArg<TArg> : IComparable<ThreadArg<TArg>>
    {
        public TArg arg;
        public bool isMainThread;

        public int CompareTo(ThreadArg<TArg> other)
            => (isMainThread ? 1 : 0) - (other.isMainThread ? 1 : 0);
    }
}