using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    internal sealed class OneThreadProcessing<TThreadScheduler> : ISystemProcessing
        where TThreadScheduler : struct, IThreadScheduler
    {
        private MultiThreadProcessing<TThreadScheduler> _impl;

        public OneThreadProcessing(State state, TThreadScheduler threadScheduler)
        {
            _impl = new MultiThreadProcessing<TThreadScheduler>(state, 1, threadScheduler);
        }

        public void StateTickStart()
        {
            _impl.StateTickStart();
        }

        public void StateTickFinished()
        {
            _impl.TickFinished();
        }

        public void Prepare(IGroupSystem systemGroup)
        {
            _impl.Prepare(systemGroup);
        }

        public void Construct()
        {
            _impl.Construct();
        }

        public void TickStart()
        {
            _impl.TickStart();
        }

        public void TickFinished()
        {
            _impl.TickFinished();
        }

        public void Init()
        {
            _impl.Init();
        }

        public void Tick()
        {
            _impl.Tick();
        }

        public void Destroy()
        {
            _impl.Destroy();
        }

        public void Receive()
        {
            _impl.Receive();
        }

        public void RevertTo(uint tick)
        {
            _impl.RevertTo(tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _impl.IsBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _impl.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
            => _impl.IsDeterministicSequence();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetParallelMax()
            => _impl.GetParallelMax();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread()
        {
            _impl.CallFromMainThread();
        }

        public void Dispose()
        {
            _impl.Dispose();
        }

        public void TickFullLoop()
        {
            _impl.TickFullLoop();
        }
    }
}

