using AnotherECS.Core;

namespace AnotherECS.Views.Core
{
    public interface IViewFactory
    {
        string GetGUID();
        IView Create();
    }

    public interface IView
    {
        void Construct(State state, in Entity entity);
        void Created();
        void Apply();
        void Destroyed();
    }
}
