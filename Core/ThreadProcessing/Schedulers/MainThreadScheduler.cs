using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Threading;

namespace AnotherECS.Core.Processing
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct MainThreadScheduler : IThreadScheduler<Task>, IDisposable
    {
        public int ParallelMax
        {
            get => 1;
            set { }
        }

        public static MainThreadScheduler Create()
            => new()
            {
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Task task)
        {
            task.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Span<Task> tasks)
        {
            for (int i = 0; i < tasks.Length; ++i)
            {
                Run(tasks[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInWork()
            => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }
    }
}