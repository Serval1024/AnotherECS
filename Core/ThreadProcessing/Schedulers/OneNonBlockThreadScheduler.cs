using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct OneNonBlockThreadScheduler : IThreadScheduler, IDisposable
    {
        private ThreadWorker _worker;
        private Queue<TaskDeferred> _tasks;

        public int ParallelMax
        {
            get => _worker.Count;
            set { }
        }

        public static OneNonBlockThreadScheduler Create()
            => new()
            {
                _worker = new ThreadWorker(1),
                _tasks = new Queue<TaskDeferred>(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            EnqueueMain<THandler, TData>(tasks, mainThreadIndex, _tasks);
            Enqueue<THandler, TData>(tasks, mainThreadIndex, _tasks);
            TryAsyncContinue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(ThreadArg<TData> task)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            Enqueue(new Task<THandler, TData>() { arg = task.arg }, task.isMainThread, _tasks);
            TryAsyncContinue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _worker.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            _worker.Wait();
        }

        public void CallFromMainThread()
        {
            if (!_worker.IsBusy())
            {
                while (_tasks.Count > 0)
                {
                    var task = _tasks.Dequeue();

                    if (task.isMainThread)
                    {
                        task.task.Invoke();
                    }
                    else
                    {
                        _worker.Schedule(task.task);

                        while (_tasks.Count > 0)
                        {
                            task = _tasks.Dequeue();

                            if (task.IsBreaker || task.isMainThread)
                            {
                                return;
                            }
                            else
                            {
                                _worker.Schedule(task.task);
                            }
                        }
                    }
                }
            }
        }

        public void SyncDispose()
        {
            _worker.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _worker.IsBusy() || _tasks.Count > 0;

        public void Dispose()
        {
            SyncDispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryAsyncContinue()
        {
            while (_tasks.Count > 0)
            {
                var task = _tasks.Peek();
                if (task.IsBreaker || task.isMainThread)
                {
                    return;
                }
                else
                {
                    _tasks.Dequeue();
                    _worker.Schedule(task.task);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Enqueue<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex, Queue<TaskDeferred> buffer)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            for (int i = 0; i < mainThreadIndex; i++)
            {
                buffer.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, false));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMain<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex, Queue<TaskDeferred> buffer)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            for (int i = mainThreadIndex; i < tasks.Length; i++)
            {
                buffer.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, true));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Enqueue<THandler, TData>(Task<THandler, TData> task, bool isMainThread, Queue<TaskDeferred> buffer)
           where THandler : struct, ITaskHandler<TData>
           where TData : struct
        {
            buffer.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = task.arg }, isMainThread));
        }
    }
}