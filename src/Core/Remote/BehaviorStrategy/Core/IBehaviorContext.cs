﻿using AnotherECS.SyncTask;

namespace AnotherECS.Core.Remote
{
    public interface IBehaviorContext
    {
        Player LocalPlayer { get; }
        Player[] Players { get; }
        Player[] OtherPlayers { get; }
        double Ping { get; }

        bool IsHasWorldValid { get; }

        IWorldExtend World { get; }
        
        void SendState(StateRequest stateRequest);
        void SendState(Player player, SerializationLevel serializationLevel);
        STask<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel);
        void ApplyWorldData(WorldData data);
        void Disconnect();
    }

    public enum ClientRole : byte
    {
        None = 0,
        Unknow,
        Master,
        Client,
    }

    public enum SerializationLevel : byte
    {
        None = 0,
        Data,
        DataAndConfig,
        DataAndConfigAndSystems,
    }
}
