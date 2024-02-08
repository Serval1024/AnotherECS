using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    [Serialize]
    public interface IComponent { }

    [Serialize]
    public interface ISingle : IComponent { }

    [Serialize]
    public interface IVersion : IComponent { }

    [Serialize]
    public interface IMarker : IComponent { }

    [Serialize]
    public interface IConfig { }

    [Serialize]
    public interface IDefault : IComponent
    {
        void Setup(State state);
    }

    [Serialize]
    public interface IAttach
    {
        void OnAttach(State state);
    }

    [Serialize]
    public interface IDetach
    {
        void OnDetach(State state);
    }
}
