using AnotherECS.Core.Processing;
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
            _systemProcessing.Prepare(systemGroup);

            state.SetOption(new StateOption()
            {
                isMultiThreadMode = !IsDeterministicSequence(),
                parallelMax = GetParallelMax(),
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            _systemProcessing.CreateModule();
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
            _systemProcessing.DestroyModule();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryRevertTo(uint currentTick, uint getNextTickForEvent)
        {
            if (currentTick >= getNextTickForEvent)
            {
                _systemProcessing.RevertTo(getNextTickForEvent - 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            _systemProcessing.RevertTo(tick);
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
            => _systemProcessing.Wait();

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
            _systemProcessing.Dispose();
        }
    }
}