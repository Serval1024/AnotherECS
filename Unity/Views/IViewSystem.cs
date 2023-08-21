using AnotherECS.Core;
using EntityId = System.Int32;

namespace AnotherECS.Views
{
    public interface IViewSystem : ISystem
    {
        uint GetId<T>()
            where T : IView;
        void Create(State state, EntityId id, uint viewId);
        void Change(EntityId id);
        void Destroy(EntityId id);
    }
}
