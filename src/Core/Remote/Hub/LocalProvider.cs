using System;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote.Local
{
    public class LocalProvider : IRemoteProvider
    {
        public event ReceiveBytesHandler ReceiveBytes;
        public event ConnectPlayerHandler ConnectPlayer;
        public event DisconnectPlayerHandler DisconnectPlayer;

        internal LocalHubProvider Parent { get; set; }

        public Player Player { get; private set; }


        public LocalProvider(Player player)
        {
            Player = player;
        }

        public Task<ConnectResult> Connect()
            => Task.FromResult(new ConnectResult(null));

        public Task Disconnect()
        {
            if (Parent != null)
            {
                Parent.Disconnect(Player);

                Player = default;
                Parent = null;
                ReceiveBytes = null;
            }

            return Task.CompletedTask;
        }

        public void SendOther(byte[] bytes)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            Parent.SendOther(Player, bytes);
        }

        public void Send(byte[] bytes)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            ReceiveBytes.Invoke(Player, bytes);
        }

        public void Send(Player player, byte[] bytes)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            Parent.Send(player, bytes);
        }

        public void Connect(Player player)
        {
            ConnectPlayer?.Invoke(player);
        }

        public void Disconnect(Player player)
        {
            DisconnectPlayer?.Invoke(player);
        }

        public Player GetLocalPlayer()
            => Player;

        public Player[] GetPlayers()
            => Parent?.GetPlayers();

        public double GetGlobalTime()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }

            return Parent.GetGlobalTime();
        }
    }
}
