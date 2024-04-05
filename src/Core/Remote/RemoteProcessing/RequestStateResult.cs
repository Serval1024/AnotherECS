namespace AnotherECS.Core.Remote
{
    public readonly struct RequestStateResult
    {
        public readonly State state;

        public RequestStateResult(State state)
        {
            this.state = state;
        }
    }
}
