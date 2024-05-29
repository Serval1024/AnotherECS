using AnotherECS.SyncTask;
using System;
using System.Linq;

namespace AnotherECS.Core.Remote
{
    internal class BehaviorContext : IBehaviorContext
    {
        public Player LocalPlayer => _remote.GetLocalPlayer();
        public Player[] Players => _remote.GetPlayers();
        public Player[] OtherPlayers
        {
            get
            {
                var localPlayerId = LocalPlayer.Id;
                return _remote.GetPlayers().Where(p => p.Id != localPlayerId).ToArray();
            }
        }

        public double Ping => _remote.GetPing();
        public bool IsHasWorldValid => World != null && !World.WorldData.IsEmpty; 
        public IWorldExtend World => _processing.GetWorld();


        private readonly IRemoteProcessing _processing;
        private readonly IRemoteProvider _remote;
        private bool _isCheckRejectRequestState;

        public BehaviorContext(IRemoteProcessing processing, IRemoteProvider remote)
        {
            _processing = processing;
            _remote = remote;
            _isCheckRejectRequestState = false;
        }

        public void Disconnect()
        {
            _processing.Disconnect();
        }

        public void SendState(StateRequest stateRequest)
        {
            _isCheckRejectRequestState = false;

            _processing.SendState(stateRequest);
        }

        public void SendState(Player player, SerializationLevel serializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }

            _isCheckRejectRequestState = false;
            
            _processing.SendState(player, serializationLevel);
        }

        public STask<RequestStateResult> RequestState(Player player, SerializationLevel serializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }

            return _processing.RequestState(player, serializationLevel);
        }

        public void ApplyWorldData(WorldData data)
        {
            _processing.ApplyWorldData(data);
        }

        public void SendReject(StateRequest stateRequest)
        {
            _processing.SendRejectState(stateRequest);   
        }

        internal void BeginCheckRejectRequestState()
        {
            _isCheckRejectRequestState = true;
        }

        internal bool EndCheckRejectRequestState()
        {
            var result = _isCheckRejectRequestState;
            _isCheckRejectRequestState = false;
            return result;
        }
    }
}
