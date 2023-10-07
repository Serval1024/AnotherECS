namespace AnotherECS.Core
{
    internal interface IRevert 
    {
        void RevertTo(uint tick, State state);
    }
}