﻿using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public interface IBehaviorContext
    {
        Player LocalPlayer { get; }
        Player[] Players { get; }
        Player[] OtherPlayers { get; }
        bool IsHasWorldValid { get; }
        LiveState WorldLiveState { get; }

        void SendState(StateRequest stateRequest);
        void SendState(Player player, SerializationLevel serializationLevel);
        Task<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel);
        public void ApplyWorldData(WorldData data);
        void Disconnect();
    }

    public enum ClientRole : byte
    {
        Unknow,
        Master,
        Client,
    }

    public enum SerializationLevel : byte
    {
        None,
        StateData,
        StateDataAndConfig,
        World,
    }
}
