using System;

namespace AnotherECS.Core
{
    public interface ISystemProcessing : IDisposable
    {
        void Prepare(IGroupSystem systemGroup);

        void Construct();
        void TickStart();
        void TickFinished();
        
        void Init();
        void Tick();
        void Destroy();

        void Receive();

        void CallFromMainThread();
    }
}