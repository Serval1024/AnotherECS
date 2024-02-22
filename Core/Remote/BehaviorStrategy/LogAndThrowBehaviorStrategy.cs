namespace AnotherECS.Core.Remote
{
    public class LogAndThrowBehaviorStrategy : IRemoteBehaviorStrategy
    {
        public void OnPlayerConnected(ref BehaviorContext context)
        {
            Debug.Logger.Send("Player connected.");
        }

        public void OnPlayerDisconnected(ref BehaviorContext context)
        {
            Debug.Logger.Send("Player disconnected.");
        }

        public void OnReceiveCorruptedData(ref BehaviorContext context)
        {
            Debug.Logger.ReceiveCorruptedData(context.Error.Message + " => " + context.Error.StackTrace);
            throw context.Error;
        }

        public void OnRevertFailed(ref BehaviorContext context)
        {
            Debug.Logger.RevertStateFail(context.Error.Message);
            throw context.Error;
        }
    }
}
