﻿using AnotherECS.Core.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal readonly struct LoopProcessing : IDisposable
    {
        private readonly ISystemProcessing _systemProcessing;

        public ISystemProcessing SystemProcessing
            => _systemProcessing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LoopProcessing(ISystemProcessing systemProcessing)
        {
            _systemProcessing = systemProcessing;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepare(State state, IEnumerable<ISystem> systemGroup)
        {
            _systemProcessing.Prepare(state, systemGroup);
            state.SetOption(new StateOption()
            {
                isMultiThreadMode = !IsDeterministicSequence(),
                parallelMax = GetParallelMax(),
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StateStartup()
        {
            _systemProcessing.StateStartup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create()
        {
            _systemProcessing.Create();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick()
        {
            _systemProcessing.TickFullLoop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy()
        {
            _systemProcessing.Destroy();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachToStateModule()
        {
            _systemProcessing.AttachToStateModule();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DetachToStateModule()
        {
            _systemProcessing.DetachToStateModule();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint TryRevertTo(uint currentTick, uint getNextTickForEvent)
        {
            if (currentTick >= getNextTickForEvent)
            {
                _systemProcessing.RevertTo(getNextTickForEvent - 1);
                return currentTick - (getNextTickForEvent - 1);
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            _systemProcessing.RevertTo(tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
            _systemProcessing.RevertFinished();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread()
        {
            _systemProcessing.CallFromMainThread();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _systemProcessing.IsBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _systemProcessing.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(RunTaskHandler runTaskHandler)
        {
            _systemProcessing.Run(runTaskHandler);
        }

        public void BreakAndWait()
        {
            _systemProcessing.Clear();
            _systemProcessing.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
            => _systemProcessing.IsDeterministicSequence();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetParallelMax()
            => _systemProcessing.GetParallelMax();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            BreakAndWait();
            DetachToStateModule();
            _systemProcessing.Dispose();
        }
    }
}