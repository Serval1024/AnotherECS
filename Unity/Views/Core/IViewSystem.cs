using AnotherECS.Core;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    public interface IViewSystem : ISystem
    {
        ViewId GetId<T>()
            where T : IViewFactory;
        ViewId GetId(ViewGuid guid);
        void Create(EntityId id, ViewId viewId);
        void Change(EntityId id);
        void Destroy(EntityId id);
    }
}
