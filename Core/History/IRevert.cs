namespace AnotherECS.Core
{
    internal interface IRevert 
    {
        void RevertTo(uint tick, State state);
    }

    internal interface ITickFinished
    {
        void TickFinished();
    }
}