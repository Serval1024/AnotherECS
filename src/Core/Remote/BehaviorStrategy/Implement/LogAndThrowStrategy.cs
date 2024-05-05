namespace AnotherECS.Core.Remote
{
    public class LogAndThrowStrategy : IRemoteSyncStrategy
    {
        public void OnPlayerConnected(IBehaviorContext context, Player player)
        {
            Debug.Logger.Send($"Player connected: '{player.Id}'.");
        }

        public void OnPlayerDisconnected(IBehaviorContext context, Player player)
        {
            Debug.Logger.Send($"Player disconnected: '{player.Id}'.");
        }

        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.ReceiveCorruptedData(error.Exception.Message + " => " + error.Exception.StackTrace);
            throw error.Exception;
        }

        public void OnReceiveState(IBehaviorContext context, Player sender, RequestStateResult requestStateResult)
        {
            Debug.Logger.Send($"Receive state: '{sender.Id}'.");
        }

        public void OnRequestState(IBehaviorContext context, Player sender, StateRequest stateRequest)
        {
            Debug.Logger.Send($"Request state: '{sender.Id}', level '{stateRequest.SerializationLevel}'.");
            // Empty method is auto reject request.
        }

        public void OnRevertFailed(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.RevertStateFail(error.Exception.Message);
            throw error.Exception;
        }
    }
}
