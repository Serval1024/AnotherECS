using System;

namespace AnotherECS.Core.Remote
{
    public interface IRemoteBytesProvider : IDisposable
    {
        void SendOther(byte[] bytes);
        event ReceiveBytesHandler ReceiveOtherBytes;
    }

    public delegate void ReceiveBytesHandler(byte[] bytes);

}
