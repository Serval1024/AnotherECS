using System;

namespace AnotherECS.Core.Remote
{
    public class RejectRequestStateException : Exception
    {
        public RejectRequestStateException()
            : base("The player rejected the state request.")
        {
        }      
    }
}