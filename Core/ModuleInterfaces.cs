namespace AnotherECS.Core
{
    public interface IModule : ISystem { }
    

    public interface IConstructModule : IModule
    {
        void Construct(State state);
    }

    public interface ITickStartModule : IModule
    {
        void TickStarted(State state);
    }

    public interface ITickFinishiedModule : IModule
    {
        void TickFinishied(State state);
    }
}