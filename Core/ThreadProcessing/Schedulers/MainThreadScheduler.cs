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

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        public ITimerStatistic Statistic { get; set; }
#endif

        public static MainThreadScheduler Create()
            => new() { };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Task task)
        {
            StartTimer(ref task);
            task.Invoke();
            StopTimer(ref task);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartTimer(ref Task task)
        {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            if (Statistic != null)
            {
                Statistic.StartTimer(task.id);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopTimer(ref Task task)
        {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            if (Statistic != null)
            {
                Statistic.StopTimer(task.id);
            }
#endif
        }
    }
}