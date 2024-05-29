using System;
using AnotherECS.SyncTask;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteProcessing : IDisposable
    {
        void Construct(IWorldInner world);
        void Update();

        void SendOtherEvent(ITickEvent data);
        void Send(Player target, object data);

        STask<ConnectResult> Connect();
        STask Disconnect();

        void SendState(StateRequest stateRequest);
        void SendState(Player target, SerializationLevel serializationLevel);
        void SendRejectState(StateRequest stateRequest);
        STask<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel);

        IWorldExtend GetWorld();
        void ApplyWorldData(WorldData worldData);

        Player GetLocalPlayer();
        double GetGlobalTime();
        uint GetEventTickСorrection(uint tick);
    }
}
