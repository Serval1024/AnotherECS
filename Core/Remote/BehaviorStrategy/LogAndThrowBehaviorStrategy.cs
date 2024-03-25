namespace AnotherECS.Core.Remote
{
    public class LogAndThrowBehaviorStrategy : IRemoteBehaviorStrategy
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

        public void OnReceiveState(IBehaviorContext context, Player sender, State state)
        {
            Debug.Logger.Send($"Receive state: '{sender.Id}'.");
        }

        public void OnRequestState(IBehaviorContext context, Player sender, StateSerializationLevel level)
        {
            Debug.Logger.Send($"Request state: '{sender.Id}', level '{level}'.");
        }

        public void OnRevertFailed(IBehaviorContext context, ErrorReport error)
        {
            Debug.Logger.RevertStateFail(error.Exception.Message);
            throw error.Exception;
        }
    }
}
