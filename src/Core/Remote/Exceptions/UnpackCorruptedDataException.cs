using System;

namespace AnotherECS.Core.Remote.Exceptions
{
    public class UnpackCorruptedDataException : Exception
    {
        public Player SenderPlayerId { get; }

        public UnpackCorruptedDataException(Player senderPlayerId, Exception innerException)
            : base($"Fail to unpack data.", innerException)
        {
            SenderPlayerId = senderPlayerId;
        }
    }
}