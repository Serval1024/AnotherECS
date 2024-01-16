using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Threading;

namespace AnotherECS.Core.Processing
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct OneNonBlockThreadScheduler : IThreadScheduler<Task>, IDisposable
    {
        private ThreadWorker<Task> _worker;
        private Queue<Task> _tasks;

        public int ParallelMax
        {
            get => _worker.Count;
            set { }
        }

        public static OneNonBlockThreadScheduler Create()
            => new()
            {
                _worker = new ThreadWorker<Task>(1),
                _tasks = new Queue<Task>(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Task task)
        {
            _tasks.Enqueue(task);
            TryAsyncContinue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Span<Task> tasks)
        {
            for (int i = 0; i < tasks.Length; ++i)
            {
                _tasks.Enqueue(tasks[i]);
            }
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
            while (_tasks.Count > 0)
            {
                _tasks.Dequeue().Invoke();
            }
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
                        task.Invoke();   
                    }
                    else
                    {
                        _worker.Schedule(task);

                        while (_tasks.Count > 0)
                        {
                            task = _tasks.Dequeue();

                            if (task.isMainThread)
                            {
                                return;
                            }
                            else
                            {
                                _worker.Schedule(task);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInWork()
            => _worker.GetInWork();

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
                if (task.isMainThread)
                {
                    return;
                }
                else
                {
                    _tasks.Dequeue();
                    _worker.Schedule(task);
                }
            }
        }
    }
}