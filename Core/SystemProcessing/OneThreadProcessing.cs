using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    internal sealed class OneThreadProcessing : ISystemProcessing
    {
        private MultiThreadProcessing _impl;

        public OneThreadProcessing(State state, IThreadScheduler threadScheduler)
        {
            _impl = new MultiThreadProcessing(state, 1, threadScheduler);
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
        public bool IsDeterministicSequence()
            => true;

        public void CallFromMainThread()
        {
            _impl.CallFromMainThread();
        }

        public void Dispose()
        {
            _impl.Dispose();
        }
    }
}

