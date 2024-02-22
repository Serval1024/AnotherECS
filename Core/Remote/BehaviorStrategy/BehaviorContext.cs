using System;

namespace AnotherECS.Core.Remote
{
    public struct BehaviorContext
    {
        public Exception Error { get; private set; }
        public ClientRole Role { get; private set; }
        public bool IsMaster => Role == ClientRole.Master;

        public void Disconnect()
        {

        }

        public void SendState()
        {

        }


        public enum ClientRole
        {
            Unknow,
            Master,
            Client,
        }
    }
}
