using System;

namespace AnotherECS.Core.Remote
{
    public readonly struct ErrorReport
    {
        public uint WorldId { get; }
        public Exception Error { get; }

        public ErrorReport(uint worldId, Exception ex)
        {
            WorldId = worldId;
            Error = ex;
        }

        public bool Is<T>()
            => Error != null && typeof(T).IsAssignableFrom(Error.GetType());
    }
}
