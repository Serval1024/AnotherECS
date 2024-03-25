using System.Collections.Generic;
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
    }

    public class ConnectResult
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
