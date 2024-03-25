namespace AnotherECS.Core
{
    public interface IModule : ISystem { }
    
    public interface IAttachToStateModule : IModule
    {
        void OnAttachToStateModule(State state);
    }

    public interface IDetachToStateModule : IModule
    {
        void OnDetachToStateModule(State state);
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