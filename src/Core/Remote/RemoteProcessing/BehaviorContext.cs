using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public struct BehaviorContext : IBehaviorContext
    {
        public Player LocalPlayer => _remote.GetLocalPlayer();
        public Player[] Players => _remote.GetPlayers();
        public Player[] OtherPlayers
        {
            get
            {
                var localPlayerId = this.LocalPlayer.Id;
                return _remote.GetPlayers().Where(p => p.Id != localPlayerId).ToArray();
            }
        }

        public bool IsHasWorldValid { get => World != null; }
        public IWorldExtend World { get => _processing.GetWorld(); }
        public LiveState WorldLiveState { get => World.LiveState; }


        private readonly IRemoteProcessing _processing;
        private readonly IRemoteProvider _remote;
        private readonly object _locker;
        private bool _isCheckRejectRequestState;

        public BehaviorContext(IRemoteProcessing processing, IRemoteProvider remote)
        {
            _processing = processing;
            _remote = remote;
            _locker = new();
            _isCheckRejectRequestState = false;
        }

        public void Disconnect()
        {
            lock (_locker)
            {
                _processing.Disconnect();
            }
        }

        public void SendState(StateRequest stateRequest)
        {
            _isCheckRejectRequestState = false;
            lock (_locker)
            {
                _processing.SendState(stateRequest);
            }
        }

        public void SendState(Player player, SerializationLevel serializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }

            _isCheckRejectRequestState = false;
            lock (_locker)
            {
                _processing.SendState(player, serializationLevel);
            }
        }

        public Task<RequestStateResult> RequestState(Player player, SerializationLevel serializationLevel)
        {
            if (LocalPlayer == player)
            {
                throw new ArgumentException("Should be 'local player id != player id argument'.");
            }
            lock (_locker)
            {
                return _processing.RequestState(player, serializationLevel);
            }
        }

        public void ApplyWorldData(WorldData data)
        {
            if (data.World != null)
            {
                lock (_locker)
                {
                    _processing.ApplyWorld(data.World);
                }
            }
            else if (data.State != null)
            {
                lock (_locker)
                {
                    _processing.ApplyState(data.State);
                }
            }
        }

        public void SendReject(StateRequest stateRequest)
        {
            lock (_locker)
            {
                _processing.SendRejectState(stateRequest);
            }
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
