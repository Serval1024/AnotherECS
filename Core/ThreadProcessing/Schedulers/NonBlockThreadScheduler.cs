using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct NonBlockThreadScheduler : IThreadScheduler, IDisposable
    {
        private ThreadWorker _worker;
        private Queue<TaskDeferred> _tasks;
        private Queue<TaskDeferred> _tempBufferTasks;

        public int ParallelMax
        {
            get => _worker.Count;
            set
            {
                _worker.Count = value;
                _worker.SetCallbackOnNotBusy((value > 1) ? OnCompletedCallFromWorkThread : null);
            }
        }

        public static NonBlockThreadScheduler Create()
            => new()
            {
                _worker = new ThreadWorker(0),
                _tasks = new Queue<TaskDeferred>(),
                _tempBufferTasks = new Queue<TaskDeferred>(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {

            EnqueueMain<THandler, TData>(tasks, mainThreadIndex, _tempBufferTasks);
            Enqueue<THandler, TData>(tasks, mainThreadIndex, _tempBufferTasks);
            EnqueueBreaker(_tempBufferTasks);

            lock (_tasks)
            {
                Flush(_tempBufferTasks, _tasks);
                CallFromMainThreadInternal();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(ThreadArg<TData> task)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            Enqueue(new Task<THandler, TData>() { arg = task.arg }, task.isMainThread, _tempBufferTasks);
            EnqueueBreaker(_tempBufferTasks);

            lock (_tasks)
            {
                Flush(_tempBufferTasks, _tasks);
                CallFromMainThreadInternal();
            }
        }


        public void CallFromMainThread()
        {
            lock (_tasks)
            {
                CallFromMainThreadInternal();
            }
        }

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

        private void CallFromMainThreadInternal()
        {
            if (!_worker.IsBusy())
            {
                while (_tasks.Count > 0)
                {
                    var task = _tasks.Dequeue();

                    if (task.IsBreaker)
                    {
                        return;
                    }
                    else
                    {
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
        private void EnqueueBreaker(Queue<TaskDeferred> buffer)
        {
            buffer.Enqueue(TaskDeferred.Breaker);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Flush(Queue<TaskDeferred> source, Queue<TaskDeferred> destination)
        {
            while (source.Count > 0)
            {
                destination.Enqueue(source.Dequeue());
            }
        }

        private void OnCompletedCallFromWorkThread()
        {
            if (_tasks.Count > 0)
            {
                lock (_tasks)
                {
                    if (_tasks.Peek().IsBreaker)
                    {
                        _tasks.Dequeue();
                    }

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
            }
        }
    }
}

