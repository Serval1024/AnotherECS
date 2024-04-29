using System;

namespace AnotherECS.Core
{
    [Flags]
    public enum StateSerializationLevel : byte
    {
        None = 0,
        Data = 1 << 0,
        Config = 1 << 1,
    }
}
