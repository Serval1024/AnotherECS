namespace AnotherECS.Core.Remote
{
    public interface IRemoteSyncStrategy
    {
        public void OnPlayerConnected(IBehaviorContext context, Player player);
        public void OnPlayerDisconnected(IBehaviorContext context, Player player);
        public void OnReceiveState(IBehaviorContext context, Player sender, RequestStateResult requestStateResult);
        public void OnRequestState(IBehaviorContext context, Player sender, StateRequest stateRequest);
        public void OnReceiveCorruptedData(IBehaviorContext context, ErrorReport error);
        public void OnRevertFailed(IBehaviorContext context, ErrorReport error);
    }
}
