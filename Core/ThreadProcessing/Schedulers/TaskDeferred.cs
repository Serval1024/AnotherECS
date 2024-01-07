using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public struct TaskDeferred
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