using System;

namespace AnotherECS.Core.Remote
{
    public struct BehaviorContext : IBehaviorContext
    {
        public Player LocalPlayer => _remote.GetLocalPlayer();
        public Player[] Players => _remote.GetPlayers();

        public LiveState WorldLiveState { get; internal set; }


        private readonly IRemoteProcessing _processing;
        private readonly IRemoteProvider _remote;
        

        public BehaviorContext(IRemoteProcessing processing, IRemoteProvider remote)
        {
            _processing = processing;
            _remote = remote;
            WorldLiveState = default;
        }

        public void Disconnect()
        {
            _processing.Disconnect();
        }

        public void SendState(Player player, StateSerializationLevel stateSerializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }
            _processing.SendState(player, stateSerializationLevel);
        }

        public void RequestState(Player player, StateSerializationLevel stateSerializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }
            _processing.RequestState(player, stateSerializationLevel);
        }

        public void ApplyState(State state)
        {
            _processing.ApplyState(state);
        }
    }
}
