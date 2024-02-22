using System;

namespace AnotherECS.Core.Remote
{
    public class ChildHubProvider : IRemoteProvider, IDisposable
    {
        public uint WorldId { get; private set; }

        public event ReceiveBytesHandler ReceiveOtherBytes;

        internal HubBytesProvider Parent { get; set; }

        public ChildHubProvider(uint worldId)
        {
            WorldId = worldId;
        }

        public void SendOther(byte[] bytes)
        {
            Parent.SendOther(WorldId, bytes);
        }

        public void Send(byte[] bytes)
        {
            ReceiveOtherBytes.Invoke(bytes);
        }

        public void Error(ErrorReport error)
        {
            Parent.Error(new ErrorReport(WorldId, error.Error));
        }

        public void Dispose() { }
    }
}
