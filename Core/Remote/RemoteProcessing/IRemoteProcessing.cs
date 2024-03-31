using System;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteProcessing : IDisposable
    {
        void Construct(IWorldExtend world);
        void SendOtherEvent(ITickEvent data);
        void Send(Player target, object data);

        Task<ConnectResult> Connect();
        Task Disconnect();

        void SendState(StateRequest stateRequest);
        void SendState(Player target, StateSerializationLevel stateSerializationLevel);
        Task<RequestStateResult> RequestState(Player target, StateSerializationLevel stateSerializationLevel);
        void ApplyState(State state);

        Player GetLocalPlayer();
        double GetGlobalTime();
    }

    public readonly struct RequestStateResult
    {
        public readonly State state;

        public RequestStateResult(State state)
        {
            this.state = state;
        }
    }
}
