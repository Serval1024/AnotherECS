using System;

namespace AnotherECS.Core
{
    public interface IThreadProcessing
    {
        bool IsBusy();
        int GetInWork();
        int GetThreadMax();
        int GetWorkingThreadCount();
        void Wait();
        bool IsDeterministicSequence();
        uint GetParallelMax();

        void CallFromMainThread();
        int GetProcessingId();
    }

    public interface ISystemProcessing : IThreadProcessing, IDisposable
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

        void TickFullLoop();
    }
}