﻿using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class BlockThreadScheduler : IThreadScheduler, IDisposable
    {
        private ThreadWorker _worker;

        public int ParallelMax
        {
            get => _worker.Count;
            set => _worker.Count = value;
        }

        public BlockThreadScheduler()
        {
            _worker = new ThreadWorker(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run<THandler, TData>(Span<ThreadArg<TData>> tasks, int mainThreadIndex)
            where THandler : struct, ITaskHandler<TData>
            where TData : struct
        {
            if (tasks.Length == 1)
            {
                default(THandler).Invoke(ref tasks[0].arg);
                return;
            }

            for (int i = 0; i < mainThreadIndex; i++)
            {
                _worker.Schedule(new Task<THandler, TData>() { arg = tasks[i].arg });
            }

            for (int i = mainThreadIndex; i < tasks.Length; i++)
            {
                default(THandler).Invoke(ref tasks[i].arg);
            }

            _worker.Wait();
        }

        public void Dispose()
        {
            _worker.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update() { }
    }
}