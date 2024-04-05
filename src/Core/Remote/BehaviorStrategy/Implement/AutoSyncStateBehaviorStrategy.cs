using System;

namespace AnotherECS.Core.Remote
{
    public class AutoSyncStateBehaviorStrategy : IRemoteBehaviorStrategy
    {
        public double RequestStateTimeout = 5f;

        private readonly Lazy<State> _initStateForMasterClient;

        public AutoSyncStateBehaviorStrategy()
            : this(null) { }

        public AutoSyncStateBehaviorStrategy(Lazy<State> initStateForMasterClient)
        {
            _initStateForMasterClient = initStateForMasterClient;
        }

        public void OnPlayerConnected(IBehaviorContext context, Player player)
        {
            if (player.IsLocal)
            {
                if (player.Role == ClientRole.Master)
                {
                    if (_initStateForMasterClient != null)
                    {
                        context.ApplyState(_initStateForMasterClient.Value);
                    }
                }
            }
            else
            {
                if (player.Role == ClientRole.Client)
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

        public void OnReceiveState(IBehaviorContext context, Player sender, State state) { }

        private void RequestState(IBehaviorContext context, Player player)
        {
            context.RequestState(player, StateSerializationLevel.Data)
                .Timeout(RequestStateTimeout)
                .ContinueWith(p =>
                {
                    if (p.IsFaulted)
                    {
                        if (ExceptionHelper.ExtractRootException(p.Exception) is TimeoutException)
                        {
                            var nextPlayer = context.GetNextOtherPlayer(player);
                            RequestState(context, nextPlayer);
                        }
                    }
                    else
                    {
                        context.ApplyState(p.Result.state);
                    }
                }
                );
        }
    }
}
