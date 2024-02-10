using AnotherECS.Core.Caller;
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
    public interface IAttachExternal
    {
        void OnAttach(ref ADExternalContext context);
    }

    [Serialize]
    public interface IDetachExternal
    {
        void OnDetach(ref ADExternalContext context);
    }
}
