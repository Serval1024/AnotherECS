using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.SyncTask
{
    public class STask : Task
    {
        internal CancellationTokenSource RootCancellationTokenSource { get; private set; }
        internal SyncTaskManager SyncTaskManager { get; }

        public static STask Run(Task task, CancellationTokenSource cancellationTokenSource, SyncTaskManager syncTaskManager = null)
        {
            var mTask = new STask(task, cancellationTokenSource, syncTaskManager)
            {
                RootCancellationTokenSource = cancellationTokenSource
            };
            mTask.Start();
            return mTask;
        }

        public STask(Task task, CancellationTokenSource cancellationTokenSource, SyncTaskManager syncTaskManager = null)
            : base(() => { task.Wait(); cancellationTokenSource.Dispose(); })
        {
            SyncTaskManager = syncTaskManager;
            RootCancellationTokenSource = cancellationTokenSource;
        }

        public STask ContinueWithMainThread(Action<STask> continuationAction)
        {
            if (SyncTaskManager == null)
            {
                throw new InvalidOperationException($"The {nameof(SyncTaskManager)} is not assigned.");
            }

            return new STask(ContinueWith(p =>
                { SyncTaskManager.Schedule(continuationAction, this); RootCancellationTokenSource.Cancel(); },
                RootCancellationTokenSource.Token), 
                RootCancellationTokenSource, 
                SyncTaskManager);
        }
    }

    public class STask<TResult> : Task<TResult>
    {
        internal CancellationTokenSource RootCancellationTokenSource { get; private set; }
        internal SyncTaskManager SyncTaskManager { get; }

        public static STask<TResult> Run(Task<TResult> task, CancellationTokenSource cancellationTokenSource, SyncTaskManager syncTaskManager = null)
        {
            var mTask = new STask<TResult>(task, cancellationTokenSource, syncTaskManager)
            {
                RootCancellationTokenSource = cancellationTokenSource
            };
            mTask.Start();
            return mTask;
        }

        public STask(Task<TResult> task, CancellationTokenSource cancellationTokenSource, SyncTaskManager syncTaskManager = null)
            : base(() => { task.Wait(); cancellationTokenSource.Dispose(); return task.Result; })
        {
            SyncTaskManager = syncTaskManager;
            RootCancellationTokenSource = cancellationTokenSource;
        }

        public STask ContinueWithMainThread(Action<STask<TResult>> continuationAction)
        {
            if (SyncTaskManager == null)
            {
                throw new InvalidOperationException($"The {nameof(SyncTaskManager)} is not assigned.");
            }

            return new STask(ContinueWith(p =>
                { SyncTaskManager.Schedule(continuationAction, this); RootCancellationTokenSource.Cancel(); },
                RootCancellationTokenSource.Token), RootCancellationTokenSource,
                SyncTaskManager);
        }
    }

}

