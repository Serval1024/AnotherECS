namespace AnotherECS.Core.Remote
{
    public readonly struct StatusReport
    {
        public IWorldExtend World { get; }
        public ErrorReport Error { get; }

        public StatusReport(IWorldExtend world, ErrorReport error = default)
        {
            World = world;
            Error = error;
        }

        public bool IsOk
            => Error.Exception != null;

        public bool IsFaulted
            => !IsOk;
    }
}
