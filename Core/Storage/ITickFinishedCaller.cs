namespace AnotherECS.Core
{
    internal interface ITickFinishedCaller
    {
        void TickFinished();
    }

    internal interface IRevertFinishedCaller
    {
        void RevertFinished();
    }
}