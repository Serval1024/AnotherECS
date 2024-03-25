namespace AnotherECS.Core.Remote
{
    public class DefaultBehaviorStrategy : IRemoteBehaviorStrategy
    {
        private RequestStatus _requestStateStatus;

        public void OnPlayerConnected(IBehaviorContext context, Player player)
        {
            if (!_requestStateStatus.isPending)
            {
                if (context.LocalPlayer != player)
                {
                    _requestStateStatus.isPending = true;
                    _requestStateStatus.player = player;
                    context.RequestState(player, StateSerializationLevel.DataAndConfig);
                }
            }
        }

        public void OnPlayerDisconnected(IBehaviorContext context, Player player) { }

        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.ReceiveCorruptedData(error.Exception.Message + " => " + error.Exception.StackTrace);
            throw error.Exception;
        }

        public void OnReceiveState(IBehaviorContext context, Player sender, State state)
        {
            if (_requestStateStatus.isPending && _requestStateStatus.player == sender)
            {
                 context.ApplyState(state);
            }
        }

        public void OnRequestState(IBehaviorContext context, Player sender, StateSerializationLevel level)
        {
            // TODO SER
        }

        public void OnRevertFailed(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.RevertStateFail(error.Exception.Message);
            throw error.Exception;
        }


        private struct RequestStatus
        {
            public bool isPending;
            public Player player;
        }

    }
}
