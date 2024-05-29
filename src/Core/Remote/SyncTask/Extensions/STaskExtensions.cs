using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.SyncTask
{
    public static class STaskExtensions
    {
        public static STask<TResult> Timeout<TResult>(this STask<TResult> task, double timeoutSeconds)
            => task.Timeout(TimeSpan.FromSeconds(timeoutSeconds));

        public static STask<TResult> Timeout<TResult>(this STask<TResult> task, TimeSpan timeout)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            return task.SyncTaskManager.ToSTask(
                Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token))
                .ContinueWith(p =>
                {
                    var result = p.Result;

                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();

                    if (result == task)
                    {
                        return ((STask<TResult>)result).Result;
                    }
                    else
                    {
                        throw new TimeoutException("The operation has timed out.");
                    }
                }
                ), task.RootCancellationTokenSource);
        }
    }
}

