using System.Runtime.CompilerServices;
using AnotherECS.Core.Threading;

namespace AnotherECS.Core.Processing
{
    public struct Task : ITask
    {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        public string id;
#endif
        public ITaskHandler handler;
        public bool isMainThread;

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        public Task(ITaskHandler handler, bool isMainThread)
            : this(null, handler, isMainThread) { }

        public Task(string id, ITaskHandler handler, bool isMainThread)
        {
            this.id = id;
            this.handler = handler;
            this.isMainThread = isMainThread;
        }
#else
        public Task(ITaskHandler handler, bool isMainThread)
        {
            this.handler = handler;
            this.isMainThread = isMainThread;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            handler.Invoke();
        }
    }

    public interface ITaskHandler
    {
        void Invoke();
    }
}