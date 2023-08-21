using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class InvalideNodeTopologyException : Exception
    {
        public int Id0 { get; private set; }
        public int Id1 { get; private set; }

        public InvalideNodeTopologyException(int id0, string message)
            : base($"{DebugConst.TAG}{message}.")
        { 
            Id0 = id0;
        }

        public InvalideNodeTopologyException(int id0, int id1, string message)
            : base($"{DebugConst.TAG}{message}.")
        {
            Id0 = id0;
            Id1 = id1;
        }
    }
}
