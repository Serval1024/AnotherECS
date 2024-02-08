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
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private ThreadWorker<Task, StatisticObserver> _worker;
#else
        private ThreadWorker<Task, NoObserver<Task>> _worker;
#endif
        private Queue<Task> _tasks;
        private ITimerStatistic _statistic;

        public int ParallelMax
        {
            get => _worker.Count;
            set { }
        }

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        public ITimerStatistic Statistic
        { 
            get => _statistic;
            set
            {
                if (_statistic != value)
                {
                    _statistic = value;
                    _worker.Observer = new StatisticObserver(Statistic);
                }
            }
        }
#endif

        public static OneNonBlockThreadScheduler Create()
            => new()
            {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
                _worker = new ThreadWorker<Task, StatisticObserver>(1),
#else
                _worker = new ThreadWorker<Task, NoObserver<Task>>(1),
#endif
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
                var task = _tasks.Dequeue();
                Invoke(ref task);
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
                        Invoke(ref task);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Invoke(ref Task task)
        {
            StartTimer(ref task);
            task.Invoke();
            StopTimer(ref task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartTimer(ref Task task)
        {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            if (Statistic != null)
            {
                Statistic.StartTimer(task.id);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopTimer(ref Task task)
        {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            if (Statistic != null)
            {
                Statistic.StopTimer(task.id);
            }
#endif
        }

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        private struct StatisticObserver : IWorkObserver<Task>
        {
            private ITimerStatistic _statistic;

            public StatisticObserver(ITimerStatistic statistic)
            {
                _statistic = statistic;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnStartedTask(ref Task task)
            {
                _statistic.StartTimer(task.id);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnFinishedTask(ref Task task)
            {
                _statistic.StopTimer(task.id);
            }
        }
    }
#endif
}