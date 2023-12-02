namespace AnotherECS.Core
{
    public interface IComponent { }
    public interface IShared : IComponent { }
    public interface IVersion : IComponent { }
    public interface IMarker : IComponent { }
    public interface ICopyable : IComponent { }
    public interface IConfig { }

    public interface IDefault : IComponent
    {
        void Setup();
    }

    public interface ICopyable<T> : ICopyable
       where T : unmanaged
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

    public interface IManualRevert<TSegment>
    {
        void OnRevert(uint index, TSegment segment);
    }
}
