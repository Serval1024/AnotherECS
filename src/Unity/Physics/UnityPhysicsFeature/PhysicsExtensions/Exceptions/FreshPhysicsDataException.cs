using System;

namespace AnotherECS.Physics.Exceptions
{
    public class FreshPhysicsDataException : Exception
    {
        public FreshPhysicsDataException(uint currentTick, uint dataTick)
            : base($"PhysicsData is not up to date. {nameof(PhysicsSystem)} must be called before accessing physics data. Current tick: '{currentTick}'. PhysicsData tick: '{dataTick}'.")
        { }
    }
}