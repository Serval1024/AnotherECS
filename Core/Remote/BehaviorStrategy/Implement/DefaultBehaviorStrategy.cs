using System;

namespace AnotherECS.Core.Remote
{
    public class DefaultBehaviorStrategy : IRemoteBehaviorStrategy
    {
        public void OnPlayerConnected(IBehaviorContext context, Player player)
        {
            if (player.Role != ClientRole.Master)
            {
                RequestState(context, player);
            }
        }

        public void OnPlayerDisconnected(IBehaviorContext context, Player player) { }

        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.ReceiveCorruptedData(error.Exception.Message + " => " + error.Exception.StackTrace);
            throw error.Exception;
        }

        public void OnReceiveState(IBehaviorContext context, Player sender, State state) { }

        public void OnRequestState(IBehaviorContext context, Player sender, StateRequest stateRequest)
        {
            context.SendState(stateRequest);
        }

        public void OnRevertFailed(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.RevertStateFail(error.Exception.Message);
            throw error.Exception;
        }


        private void RequestState(IBehaviorContext context, Player player)
        {
            context.RequestState(player, StateSerializationLevel.Data)
                .Timeout(5f)
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
