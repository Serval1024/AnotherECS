using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public interface IBehaviorContext
    {
        Player LocalPlayer { get; }
        Player[] Players { get; }
        LiveState WorldLiveState { get; }

        void SendState(StateRequest stateRequest);
        void SendState(Player player, StateSerializationLevel stateSerializationLevel);
        Task<RequestStateResult> RequestState(Player target, StateSerializationLevel stateSerializationLevel);
        public void ApplyState(State state);
        void Disconnect();
    }

    public enum ClientRole : byte
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
