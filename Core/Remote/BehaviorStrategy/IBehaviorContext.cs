namespace AnotherECS.Core.Remote
{
    public interface IBehaviorContext
    {
        Player LocalPlayer { get; }
        Player[] Players { get; }
        LiveState WorldLiveState { get; }

        void SendState(Player player, StateSerializationLevel stateSerializationLevel);
        void RequestState(Player target, StateSerializationLevel stateSerializationLevel);
        public void ApplyState(State state);
        void Disconnect();
    }

    public enum ClientRole
    {
        Unknow,
        Master,
        Client,
    }

    public enum StateSerializationLevel : byte
    {
        Data,
        DataAndConfig,
    }
}
