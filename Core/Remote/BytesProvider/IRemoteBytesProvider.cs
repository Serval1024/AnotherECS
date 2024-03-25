using System;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteBytesProvider
    {
        void SendOther(byte[] bytes);
        void Send(Player target, byte[] bytes);
        event ReceiveBytesHandler ReceiveBytes;
    }

    public delegate void ReceiveBytesHandler(Player sender, byte[] bytes);
}
