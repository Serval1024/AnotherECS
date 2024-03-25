namespace AnotherECS.Core.Remote
{
    public interface IRemoteBehaviorStrategy
    {
        public void OnPlayerConnected(IBehaviorContext context, Player player);
        public void OnPlayerDisconnected(IBehaviorContext context, Player player);
        public void OnReceiveState(IBehaviorContext context, Player sender, State state);
        public void OnRequestState(IBehaviorContext context, Player sender, StateSerializationLevel level);
        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error);
        public void OnRevertFailed(IBehaviorContext context, ErrorReport error);
    }
}
