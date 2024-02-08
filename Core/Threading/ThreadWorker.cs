using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct ThreadWorker<TTask, TWorkObserver> : IDisposable
        where TTask : struct, ITask
        where TWorkObserver : struct, IWorkObserver<TTask>
    {
        private Worker[] _workers;
        private readonly Shared _shared;
        private int _workersOffset;

        public TWorkObserver Observer
        {
            get => _shared.observer;
            set => _shared.observer = value;
        }

        public int Count
        {
            get => _workers.Length;
            set
            {
                if (value != _workers.Length)
                {
                    if (value < _workers.Length)
                    {
                        for (int i = value; i < _workers.Length; ++i)
                        {
                            _workers[i].Dispose();
                        }
                        Array.Resize(ref _workers, value);
                    }
                    else
                    {
                        var lastLength = _workers.Length;
                        Array.Resize(ref _workers, value);

                        for (int i = lastLength; i < _workers.Length; ++i)
                        {
                            _workers[i] = new Worker(_shared);
                        }
                    }
                }
            }
        }

        public void SetCallbackOnNotBusy(Action onNotBusy)
        {
            if (IsBusy())
            {
                throw new InvalidOperationException("SetCallbackOnNotBusy only possible calls when all threads are waiting.");
            }

            _shared.onNotBusy = onNotBusy;
        }

        public ThreadWorker(int capacity, Action onNotBusy = null)
        {
            _workers = Array.Empty<Worker>();
            _shared = new()
            {
                onNotBusy = onNotBusy
            };
            _workersOffset = 0;
            Count = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Schedule(TTask task)
        {
            _shared.tasks.Enqueue(task);
            _shared.LockBusy();
            Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInWork()
            => _shared.GetInWork();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _shared.IsBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            while (!_shared.tasks.IsEmpty)
            {
                __TryProcessingTask(_shared);
            }
            _shared.waiterComplete.WaitOne();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _shared.waiterComplete.WaitOne();
        }

        public void Dispose()
        {
            SyncDispose();
        }

        public void SyncDispose()
        {
            _shared.tasks.Clear();
            for (int i = 0; i < _workers.Length; ++i)
            {
                _workers[i].Dispose();
            }

            for (int i = 0; i < _workers.Length; ++i)
            {
                _workers[i].WaitDispose();
            }

            _shared.waiterComplete.Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Run()
        {
            _shared.waiterComplete.Reset();
            _workersOffset = (_workersOffset + 1) % _workers.Length;
            _workers[_workersOffset].Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void __TryProcessingTask(Shared shared)
        {
            bool isExecute = false;
            while (shared.tasks.TryDequeue(out TTask task))
            {
                shared.observer.OnStartedTask(ref task);
                task.Invoke();
                shared.observer.OnFinishedTask(ref task);
                isExecute = shared.UnlockBusy();
            }

            if (isExecute)
            {
                if (shared.tasks.IsEmpty)
                {
                    if (shared.onNotBusy != null)
                    {
                        shared.onNotBusy();

                        if (shared.tasks.IsEmpty)
                        {
                            shared.waiterComplete.Set();
                        }
                    }
                    else
                    {
                        shared.waiterComplete.Set();
                    }
                }
            }
        }


#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
        private class Shared
        {
            public readonly ConcurrentQueue<TTask> tasks = new();
            public readonly ManualResetEvent waiterComplete = new(true);
            public volatile int inWork;
            public Action onNotBusy;
            public TWorkObserver observer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetInWork()
                => inWork;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsBusy()
                => GetInWork() != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LockBusy()
            {
                Interlocked.Increment(ref inWork);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool UnlockBusy()
                => Interlocked.Decrement(ref inWork) == 0;
        }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
        private class Worker : IDisposable
        {
            private readonly Shared _shared;

            private readonly Thread _thread;
            private readonly AutoResetEvent _waiterTask;

            private volatile bool _isLiving;


            public Worker(Shared shared)
            {
                _shared = shared;

                _waiterTask = new AutoResetEvent(false);
                _isLiving = true;

                _thread = default;
                _thread = new Thread(new ThreadStart(__Processing));
                _thread.Start();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsFree()
                => _thread.ThreadState == ThreadState.WaitSleepJoin;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Run()
            {
                _waiterTask.Set();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WaitDispose()
            {
                _thread.Join();
            }

            public void Dispose()
            {
                _isLiving = false;
                _shared.LockBusy();
                Run();
            }


            private void __Processing()
            {
                while (_isLiving)
                {
                    _waiterTask.WaitOne();
                    __TryProcessingTask(_shared);
                }
                _waiterTask.Dispose();
            }
        }
    }
    public interface ITask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Invoke();
    }

    internal interface IWorkObserver<TTask>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnStartedTask(ref TTask task);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnFinishedTask(ref TTask task);
    }

    internal struct NoObserver<TTask> : IWorkObserver<TTask>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnFinishedTask(ref TTask task) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStartedTask(ref TTask task) { }
    }
}