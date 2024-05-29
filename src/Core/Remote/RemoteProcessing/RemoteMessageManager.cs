using AnotherECS.SyncTask;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    internal class RemoteMessageManager : IDisposable
    {
        private const int WAIT_INTERVAL_MSECONDS = 30;
        private const int TTL_TIME_SECONDS = 30;

        private const int TTL = (int)(TTL_TIME_SECONDS * (1000f / WAIT_INTERVAL_MSECONDS));

        private bool _isDisposed;

        private uint _idCounter;

        private readonly Func<object, object> _messageProcessing;

        private readonly SyncTaskManager _syncTaskManager = new();
        private readonly ConcurrentDictionary<uint, object> _taskDataResult = new();

        public RemoteMessageManager(Func<object, object> messageProcessing)
        {
            _messageProcessing = messageProcessing;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MessageToken<TResult> BeginMessage<TResult>()
        {
            var id = unchecked(++_idCounter);
            var cancellationTokenSource = new CancellationTokenSource();
            var task = Task.Run(() => UpdateLoop<TResult>(id, cancellationTokenSource), cancellationTokenSource.Token);

            return new(id, cancellationTokenSource, ToSTask(task, cancellationTokenSource));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndMessage<TData>(uint messageId, TData data)
            where TData : struct
        {
            _taskDataResult.AddOrUpdate(messageId, data, (k, v) => data);
        }

        public STask ToSTask(Task task)
            => _syncTaskManager.ToSTask(task);

        public STask ToSTask(Task task, CancellationTokenSource cancellationTokenSource)
            => _syncTaskManager.ToSTask(task, cancellationTokenSource);

        public STask<TResult> ToSTask<TResult>(Task<TResult> task)
            => _syncTaskManager.ToSTask(task);

        public STask<TResult> ToSTask<TResult>(Task<TResult> task, CancellationTokenSource cancellationTokenSource)
            => _syncTaskManager.ToSTask(task, cancellationTokenSource);

        public void Update()
        {
            _syncTaskManager.Update();
        }

        private async Task<TResult> UpdateLoop<TResult>(object id, CancellationTokenSource cancellationTokenSource)
        {
            int ttl = TTL;
            uint messageId = (uint)id;

            while (true)
            {
                if (_isDisposed || ttl <= 0)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }

                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (_taskDataResult.TryRemove(messageId, out var result))
                {
                    return (TResult)_messageProcessing.Invoke(result);
                }

                await Task.Delay(WAIT_INTERVAL_MSECONDS);
                --ttl;
            }
            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        internal readonly struct MessageToken<TResult>
        {
            public readonly uint id;
            public readonly CancellationTokenSource cancellationTokenSource;
            public readonly STask<TResult> task;

            public MessageToken(uint id, CancellationTokenSource cancellationTokenSource, STask<TResult> task)
            {
                this.id = id;
                this.cancellationTokenSource = cancellationTokenSource;
                this.task = task;
            }
        }
    }
}