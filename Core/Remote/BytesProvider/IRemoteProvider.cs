using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteProvider : IRemoteBytesProvider 
    {
        event ConnectPlayerHandler ConnectPlayer;
        event DisconnectPlayerHandler DisconnectPlayer;

        Task<ConnectResult> Connect();
        Task Disconnect();

        Player GetLocalPlayer();
        Player[] GetPlayers();
        double GetGlobalTime();
        Player GetPlayer(long id)
        {
            var players = GetPlayers();
            if (players != null)
            {
                for (int i = 0; i < players.Length; ++i)
                {
                    if (players[i].Id == id)
                    {
                        return players[i];
                    }
                }
            }
            return default;
        }
    }

    public struct ConnectResult
    {
        public object Result { get; private set; }

        public ConnectResult(object result)
        {
            Result = result;
        }
    }

    public delegate void ConnectPlayerHandler(Player id);
    public delegate void DisconnectPlayerHandler(Player id);
}
