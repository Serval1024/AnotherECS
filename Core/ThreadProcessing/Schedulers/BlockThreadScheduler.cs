using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct BlockThreadScheduler : IThreadScheduler, IDisposable
    {
        private ThreadWorker _worker;

        public int ParallelMax
        {
            get => _worker.Count;
            set => _worker.Count = value;
        }

        public static BlockThreadScheduler Create()
            => new()
            {
                _worker = new ThreadWorker(0),
            };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            if (tasks.Length == 1)
            {
                default(THandler).Invoke(ref tasks[0].arg);
                return;
            }

            for (int i = 0; i < mainThreadIndex; i++)
            {
                _worker.Schedule(new Task<THandler, TData>() { arg = tasks[i].arg });
            }

            for (int i = mainThreadIndex; i < tasks.Length; i++)
            {
                default(THandler).Invoke(ref tasks[i].arg);
            }

            _worker.Complete();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(ThreadArg<TData> task)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            default(THandler).Invoke(ref task.arg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInWork()
            => _worker.GetInWork();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWorkingThreadCount()
           => _worker.GetWorkingThreadCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _worker.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            _worker.Complete();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread() { }

        public void Dispose()
        {
            _worker.Dispose();
        }
    }
}