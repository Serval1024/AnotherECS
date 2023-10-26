namespace AnotherECS.Core
{

    public enum Op : byte
    {
        NONE = 0,
        ADD = 1 << 0,
        REMOVE = 1 << 1,
        BOTH = ADD | REMOVE,
    }
}
