using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal readonly struct LoopProcessing : IDisposable
    {
        private readonly ISystemProcessing _systemProcessing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LoopProcessing(ISystemProcessing systemProcessing)
        {
            _systemProcessing = systemProcessing;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(IGroupSystem systemGroup)
        {
            _systemProcessing.Prepare(systemGroup);
            _systemProcessing.Construct();
            _systemProcessing.Init();
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
        public void TryRevertTo(uint currentTick, uint getNextTickForEvent)
        {
            if (getNextTickForEvent <= currentTick)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
            => _systemProcessing.IsDeterministicSequence();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _systemProcessing.Dispose();
        }
    }
}