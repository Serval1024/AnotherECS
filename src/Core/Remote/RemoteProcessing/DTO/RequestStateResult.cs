namespace AnotherECS.Core.Remote
{
    public readonly struct RequestStateResult
    {
        public readonly StateRespond Respond;

        public RequestStateResult(StateRespond respond)
        {
            Respond = respond;
        }
    }
}
