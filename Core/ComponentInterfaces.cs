namespace AnotherECS.Core
{
    public interface IComponent { }
    public interface IComponent<EState> : IComponent
        where EState : IState
    { }

    public interface IShared : IComponent { }
    public interface IVersion : IComponent { }
    public interface IMarker : IComponent { }
    public interface ICopyable : IComponent { }
    public interface ICopyable<T> : ICopyable
       where T : struct, IComponent
    {
        void CopyFrom(in T other);
        void OnRecycle();
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
