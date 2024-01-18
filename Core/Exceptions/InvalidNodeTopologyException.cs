using AnotherECS.Debug;
using System;

namespace AnotherECS.Exceptions
{
    public class InvalidNodeTopologyException : Exception
    {
        public int Id0 { get; private set; }
        public int Id1 { get; private set; }

        public InvalidNodeTopologyException(int id0, string message)
            : base($"{DebugConst.TAG}{message}.")
        { 
            Id0 = id0;
        }

        public InvalidNodeTopologyException(int id0, int id1, string message)
            : base($"{DebugConst.TAG}{message}.")
        {
            Id0 = id0;
            Id1 = id1;
        }
    }
}
