namespace AnotherECS.Core.Remote
{
    public interface IRemoteBehaviorStrategy
    {
        public void OnPlayerConnected(ref BehaviorContext context);
        public void OnPlayerDisconnected(ref BehaviorContext context);
        public void OnReceiveCorruptedData(ref BehaviorContext context);
        public void OnRevertFailed(ref BehaviorContext context);
    }
}
