namespace AnotherECS.Core.Remote
{
    public readonly struct RequestStateResult
    {
        public readonly WorldData data;

        public RequestStateResult(WorldData data)
        {
            this.data = data;
        }
    }
}
