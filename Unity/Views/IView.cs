using AnotherECS.Core;

namespace AnotherECS.Views
{
    public interface IView
    {
        void Construct(State state, in Entity entity);
        string GetGUID();
        void Created();
        void Apply();
        void Destroyed();
        IView Create();
    }
}
