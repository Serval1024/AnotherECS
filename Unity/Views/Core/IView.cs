using AnotherECS.Core;

namespace AnotherECS.Views.Core
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
