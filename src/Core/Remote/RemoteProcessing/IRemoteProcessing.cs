using System;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteProcessing : IDisposable
    {
        void Construct(IWorldComposite world);
        void SendOtherEvent(ITickEvent data);
        void Send(Player target, object data);

        Task<ConnectResult> Connect();
        Task Disconnect();

        void SendState(StateRequest stateRequest);
        void SendState(Player target, SerializationLevel serializationLevel);
        void SendRejectState(StateRequest stateRequest);
        Task<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel);

        IWorldExtend GetWorld();
        void ApplyWorld(IWorldExtend world);
        void ApplyState(State state);

        Player GetLocalPlayer();
        double GetGlobalTime();
    }
}
