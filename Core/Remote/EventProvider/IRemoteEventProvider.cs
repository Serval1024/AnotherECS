using System;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteEventProvider : IDisposable
    {
        event ReceiveEventHandler ReceiveEvent;
        event ReceiveStateHandler ReceiveState;
        void SendOtherEvent(ITickEvent data);
    }

    public delegate void ReceiveEventHandler(ITickEvent data);
    public delegate void ReceiveStateHandler(State data);
}
