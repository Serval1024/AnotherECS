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
            set => _worker.Count = value;
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
            for (int i = 0; i < mainThreadIndex; i++)
            {
                _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, false));
            }

            for (int i = mainThreadIndex; i < tasks.Length; i++)
            {
                _tasks.Enqueue(new TaskDeferred(new Task<THandler, TData>() { arg = tasks[i].arg }, true));
            }

            Update();
        }

        public void Update()
        {
            if (!_worker.IsBusy())
            {
                if (_tasks.Count > 0)
                {
                    var task = _tasks.Peek();
                    var isMainThread = task.isMainThread;

                    if (isMainThread)
                    {
                        while (_tasks.Count > 0)
                        {
                            task = _tasks.Peek();
                            if (task.isMainThread)
                            {
                                _tasks.Dequeue();
                                task.task.Invoke();
                            }
                            else
                            {
                                _worker.Schedule(_tasks.Dequeue().task);

                                while (_tasks.Count > 0)
                                {
                                    task = _tasks.Peek();
                                    if (!task.isMainThread)
                                    {
                                        _worker.Schedule(task.task);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (_tasks.Count > 0)
                        {
                            task = _tasks.Peek();
                            if (!task.isMainThread)
                            {
                                _worker.Schedule(task.task);
                            }
                            else
                            {
                                return;
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

        public void Dispose()
        {
            SyncDispose();
        }

        private struct TaskDeferred
        {
            public ITask task;
            public bool isMainThread;

            public TaskDeferred(ITask task, bool isMainThread)
            {
                this.task = task;
                this.isMainThread = isMainThread;
            }
        }
    }
}

