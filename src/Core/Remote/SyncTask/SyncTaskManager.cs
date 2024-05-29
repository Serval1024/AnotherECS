using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.SyncTask
{
    public class SyncTaskManager
    {
        private readonly ConcurrentQueue<Action> _handlers = new();

        public STask ToSTask(Task task)
            => ToSTask(task, new CancellationTokenSource());

        public STask ToSTask(Task task, CancellationTokenSource cancellationTokenSource)
        {
            var sTask = new STask(task, cancellationTokenSource, this);
            sTask.Start();
            return sTask;
        }

        public STask<TResult> ToSTask<TResult>(Task<TResult> task)
            => ToSTask(task, new CancellationTokenSource());

        public STask<TResult> ToSTask<TResult>(Task<TResult> task, CancellationTokenSource cancellationTokenSource)
        {
            var sTask = new STask<TResult>(task, cancellationTokenSource, this);
            sTask.Start();
            return sTask;
        }

        public void Update()
        {
            while (_handlers.TryDequeue(out Action handler))
            {
                handler.Invoke();
            }
        }

        internal void Schedule(Action<STask> continuationAction, STask task)
        {
            _handlers.Enqueue(() => continuationAction(task));
        }

        internal void Schedule<TResult>(Action<STask<TResult>> continuationAction, STask<TResult> task)
        {
            _handlers.Enqueue(() => continuationAction(task));
        }
    }
}

