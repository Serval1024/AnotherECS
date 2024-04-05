using AnotherECS.Core;

namespace AnotherECS.Views.Core
{
    public interface IViewFactory
    {
        ViewGuid GetGUID();
        IView Create();
    }

    public interface IView
    {
        void Construct(State state, in EntityReadOnly entity);
        void Created();
        void Apply();
        void Destroyed();
    }
}
