using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class NonBlockThreadScheduler : IThreadScheduler, IDisposable
    {
        private ThreadWorker _worker;
        private Queue<TaskDeferred> _tasks;

        public int ParallelMax
        {
            get => _worker.Count;
            set
            {
                _worker.Count = value;
                _worker.SetCallbackOnNotBusy((value > 1) ? OnCompletedCallFromWorkThread : null);
            }
        }

        public NonBlockThreadScheduler()
        {
            _worker = new ThreadWorker(0);
            _tasks = new Queue<TaskDeferred>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            if (IsMultiParallel())
            {
                lock (_tasks)
                {
                    EnqueueBreaker();
                    Enqueue<THandler, TData>(tasks, mainThreadIndex);
                    TryMiddleEnqueueBreaker(tasks, mainThreadIndex);
                    EnqueueMain<THandler, TData>(tasks, mainThreadIndex);

                    CallFromMainThreadInternal();
                }
            }
            else
            {

                TryEnqueueBreaker(tasks, 0);
                Enqueue<THandler, TData>(tasks, mainThreadIndex);
                TryEnqueueBreaker(tasks, mainThreadIndex);
                EnqueueMain<THandler, TData>(tasks, mainThreadIndex);

                TryContinue();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(ThreadArg<TData> task)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            if (IsMultiParallel())
            {
                lock (_tasks)
                {
                    EnqueueBreaker();
                    _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = task.arg }, task.isMainThread));
                    CallFromMainThreadInternal();
                }
            }
            else
            {
                TryEnqueueBreaker(task);
                _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = task.arg }, task.isMainThread));
                TryContinue();
            }
        }


        public void CallFromMainThread()
        {
            if (IsMultiParallel())
            {
                lock (_tasks)
                {
                    CallFromMainThreadInternal();
                }
            }
            else
            {
                CallFromMainThreadInternal();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMultiParallel()
            => ParallelMax > 1;

        private void TryContinue()
        {
            while (_tasks.Count > 0)
            {
                var task = _tasks.Peek();
                UnityEngine.Debug.Log(task.task);
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

        private void TryEnqueueBreaker<TData>(Span<ThreadArg<TData>> tasks, int index)
        {
            if (index < tasks.Length)
            {
                TryEnqueueBreaker(tasks[index]);
            }
        }

        private void TryEnqueueBreaker<TData>(ThreadArg<TData> task)
        {
            if (
                _tasks.Count != 0 &&
                (_tasks.Peek().isMainThread != task.isMainThread)
                )
            {
                if (_tasks.Peek().isMainThread && !task.isMainThread)
                {
                    EnqueueBreaker();
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryMiddleEnqueueBreaker<TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
        {
            if (IsMiddleEnqueueBreaker(tasks, mainThreadIndex))
            {
                EnqueueBreaker();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMiddleEnqueueBreaker<TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            => !(mainThreadIndex >= tasks.Length && mainThreadIndex <= 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueBreaker()
        {
            _tasks.Enqueue(TaskDeferred.Breaker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Enqueue<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            for (int i = 0; i < mainThreadIndex; i++)
            {
                _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, false));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMain<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            for (int i = mainThreadIndex; i < tasks.Length; i++)
            {
                _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, true));
            }
        }

        private void OnCompletedCallFromWorkThread()
        {
            if (_tasks.Count > 0)
            {
                lock (_tasks)
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
            }
        }

        private struct TaskDeferred
        {
            public readonly static TaskDeferred Breaker = default;

            public ITask task;
            public bool isMainThread;

            public bool IsBreaker
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => task == null;
            }

            public TaskDeferred(ITask task, bool isMainThread)
            {
                this.task = task;
                this.isMainThread = isMainThread;
            }
        }
    }
}

