namespace AnotherECS.Core
{
    public interface IComponent { }
    public interface ISingle : IComponent { }
    public interface IVersion : IComponent { }
    public interface IMarker : IComponent { }
    public interface IConfig { }

    public interface IDefault : IComponent
    {
        void Setup(State state);
    }

    public interface IAttach
    {
        void OnAttach(State state);
    }

    public interface IDetach
    {
        void OnDetach(State state);
    }
}
