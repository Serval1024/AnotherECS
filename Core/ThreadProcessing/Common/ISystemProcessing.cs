using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Processing
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
    }

    public interface ISystemProcessing : IThreadProcessing, IStatisticProcessing, IDisposable
    {
        void Prepare(IEnumerable<ISystem> systemGroup);

        void StateTickStart();
        void StateTickFinished();

        void CreateModule();
        void TickStart();
        void TickFinished();
        
        void Create();
        void Tick();
        void Destroy();

        void Receive();

        void RevertTo(uint tick);

        void TickFullLoop();
    }

    public interface IStatisticProcessing
    {
        void SetStatistic(ITimerStatistic timerStatistic);
    }
}