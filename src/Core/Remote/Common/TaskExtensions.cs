using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public static class TaskExtensions
    {
        public static Task<TResult> Run<TResult>(this Func<object, Task<TResult>> function, object state)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            return Task<Task<TResult>>.Factory.StartNew(function, state).Unwrap();
        }

        public static Task<TResult> Timeout<TResult>(this Task<TResult> task, double timeoutSeconds)
            => task.Timeout(TimeSpan.FromSeconds(timeoutSeconds));

        public static Task<TResult> Timeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            return Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token))
                .ContinueWith(p =>
                {
                    var result = p.Result;

                    if (result == task)
                    {
                        timeoutCancellationTokenSource.Cancel();
                        timeoutCancellationTokenSource.Dispose();

                        return ((Task<TResult>)result).Result;
                    }
                    else
                    {
                        timeoutCancellationTokenSource.Dispose();
                        throw new TimeoutException("The operation has timed out.");
                    }
                }
                );
        }
    }
}