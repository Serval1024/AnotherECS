namespace AnotherECS.Core
{
    public interface IModule : ISystem { }
    
    public interface ICreateModule : IModule
    {
        void OnCreateModule(State state);
    }

    public interface ITickStartedModule : IModule
    {
        void OnTickStarted(State state);
    }

    public interface ITickFinishedModule : IModule
    {
        void OnTickFinished(State state);
    }
}