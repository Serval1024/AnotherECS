using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AnotherECS.Core.Threading
{
    public struct ThreadWaitProvider
    {
        private object _waitLocker;
        private MRecycle _recycle;
        private List<WaiterData> _waiters;

        public static ThreadWaitProvider Create()
            => new()
            {
                _waitLocker = new object(),
                _recycle = new MRecycle(16),
                _waiters = new List<WaiterData>(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Register(IThreadProcessing processing)
        {
            lock (_waiters)
            {
                var id = _recycle.Allocate();
                while (id >= _waiters.Count)
                {
                    _waiters.Add(default);
                }
                _waiters[id] = new WaiterData() { threadData = processing };
                return id;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unregister(int id)
        {
            lock (_waiters)
            {
                _recycle.Deallocate(id);
                _waiters[id] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int id)   //Wait until other threads finished your tasks.
        {
            if (id > 0)
            {
                WaiterData waiterData;
                lock (_waiters)
                {
                    waiterData = _waiters[id];
                }

                if (waiterData.threadData.GetThreadMax() > 1)
                {
                    Interlocked.Increment(ref waiterData.threadWaitCount);
                    while (true)
                    {
                        if (waiterData.threadWaitCount >= waiterData.threadData.GetWorkingThreadCount())
                        {
                            lock (_waitLocker)
                            {
                                if (waiterData.threadWaitCount >= waiterData.threadData.GetWorkingThreadCount())
                                {
                                    if (waiterData.threadWaitCount >= waiterData.threadData.GetInWork())
                                    {
                                        Interlocked.Decrement(ref waiterData.threadWaitCount);
                                        return;
                                    }
                                }
                            }

                        }
                        Thread.Sleep(5);
                    }
                }
            }
        }

        private class WaiterData
        {
            public int threadWaitCount;
            public IThreadProcessing threadData;
        }

    }
}
