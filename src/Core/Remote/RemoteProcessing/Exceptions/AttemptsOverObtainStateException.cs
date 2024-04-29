using System;

namespace AnotherECS.Core.Remote
{
    public class AttemptsOverObtainStateException : Exception
    {
        public AttemptsOverObtainStateException()
            : base("Ended up trying to get state from other players.")
        {
        }
    }
}