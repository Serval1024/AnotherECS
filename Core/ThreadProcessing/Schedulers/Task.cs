using System.Runtime.CompilerServices;
using AnotherECS.Core.Threading;

namespace AnotherECS.Core.Processing
{
    public struct Task : ITask
    {
        public ITaskHandler handler;
        public bool isMainThread;

        public Task(ITaskHandler handler, bool isMainThread)
        {
            this.handler = handler;
            this.isMainThread = isMainThread;
        }

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