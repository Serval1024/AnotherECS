using System;

namespace AnotherECS.Core.Remote
{
    public class AutoSyncWorldByMasterStrategy : IRemoteSyncStrategy
    {
        public event Action WorldReady;
        public event Action<Exception> WorldObtainFailed;

        public double RequestStateTimeout = 5f;
        public int RequestStateTryCount = 3;

        private bool _isOneGateRequestState;
        private readonly Lazy<WorldData> _initStateForMasterClient;
        private readonly SerializationLevel _serializationLevel;

        public AutoSyncWorldByMasterStrategy(Lazy<WorldData> initStateForMasterClient = null, SerializationLevel serializationLevel = SerializationLevel.World)
        {
            _initStateForMasterClient = initStateForMasterClient;
            _serializationLevel = serializationLevel;
        }

        public void OnPlayerConnected(IBehaviorContext context, Player player)
        {
            if (player.IsLocal)     // If Master create new world.
            {
                if (context.LocalPlayer.Role == ClientRole.Master)
                {
                    if (_initStateForMasterClient != null)
                    {
                        try
                        {
                            context.ApplyWorldData(_initStateForMasterClient.Value);
                        }
                        catch (Exception ex)
                        {
                            WorldObtainFailed?.Invoke(ex);
                            return;
                        }
                        WorldReady?.Invoke();
                    }
                }
            }
            else     // If Client request world from Master.
            {
                if (context.LocalPlayer.Role == ClientRole.Client && player.Role == ClientRole.Master)
                {
                    RequestState(context, player);
                }
            }
        }

        public void OnPlayerDisconnected(IBehaviorContext context, Player player) { }

        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.ReceiveCorruptedData(error.Exception.Message + " => " + error.Exception.StackTrace);
            throw error.Exception;
        }

        public void OnRequestState(IBehaviorContext context, Player sender, StateRequest stateRequest)
        {
            context.SendState(stateRequest);
        }

        public void OnRevertFailed(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.RevertStateFail(error.Exception.Message);
            throw error.Exception;
        }

        public void OnReceiveState(IBehaviorContext context, Player sender, StateRespond stateRespond) { }

        private void RequestState(IBehaviorContext context, Player player)
        {
            if (!_isOneGateRequestState && !context.IsHasWorldValid)
            {
                _isOneGateRequestState = true;

                context.RequestState(player, _serializationLevel)
                    .Timeout(RequestStateTimeout)
                    .ContinueWith(p =>
                    {
                        if (p.IsFaulted)
                        {
                            var root = p.Exception.GetRoot();
                            if (root is TimeoutException || root is RejectRequestStateException)    // Try request world again.
                            {
                                if (RequestStateTryCount > 0)
                                {
                                    --RequestStateTryCount;

                                    _isOneGateRequestState = false;
                                    var nextPlayer = context.GetNextOtherPlayer(player);
                                    RequestState(context, nextPlayer);
                                }
                                else
                                {
                                    WorldObtainFailed?.Invoke(new AttemptsOverObtainStateException());
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                context.ApplyWorldData(p.Result.data);
                            }
                            catch (Exception ex)
                            {
                                WorldObtainFailed?.Invoke(ex);
                                return;
                            }
                            WorldReady?.Invoke();
                        }
                    }
                    );
            }
        }
    }
}
