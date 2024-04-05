namespace AnotherECS.Core
{
    public enum LiveState
    {
        Raw = 0,
        Inited = 1,
        Startup = 2,
        Destroy = 3,
        Disposing = 4,
        Disposed = 5,
    }
}