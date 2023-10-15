namespace AnotherECS.Core
{
    internal interface IRevertCaller 
    {
        void RevertTo(uint tick, State state);
    }
}