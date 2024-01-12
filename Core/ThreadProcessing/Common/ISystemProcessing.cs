using System;

namespace AnotherECS.Core
{
    public interface ISystemProcessing : IDisposable
    {
        void Prepare(IGroupSystem systemGroup);

        void StateTickStart();
        void StateTickFinished();

        void Construct();
        void TickStart();
        void TickFinished();
        
        void Init();
        void Tick();
        void Destroy();

        void Receive();

        void RevertTo(uint tick);

        bool IsBusy();
        void Wait();
        bool IsDeterministicSequence();
        uint GetParallelMax();

        void CallFromMainThread();

        void TickFullLoop();
    }
}