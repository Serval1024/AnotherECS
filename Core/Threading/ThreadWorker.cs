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
    public struct ThreadWorker : IDisposable
    {
        private Worker[] _workers;
        private readonly Shared _shared;
        private int _workersOffset;

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
        public void Schedule(ITask task)
        {
            _shared.tasks.Enqueue(task);
            _shared.LockBusy();
            Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _shared.IsAnyBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            while (!_shared.tasks.IsEmpty)
            {
                __TryProcessingTask(_shared);
            }
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
            if (shared.tasks.TryDequeue(out ITask task))
            {
                task.Invoke();
                shared.UnlockBusy();

            }
            if (!shared.IsAnyBusy())
            {
                shared.waiterComplete.Set();
                shared.onNotBusy?.Invoke();
            }
        }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
        private class Shared
        {
            public readonly ConcurrentQueue<ITask> tasks = new();
            public readonly ManualResetEvent waiterComplete = new(true);
            public int inWork;
            public Action onNotBusy;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsAnyBusy()
                => inWork != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LockBusy()
            {
                Interlocked.Increment(ref inWork);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnlockBusy()
            {
                Interlocked.Decrement(ref inWork);
            }
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
}