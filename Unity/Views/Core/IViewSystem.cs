using AnotherECS.Core;
using EntityId = System.UInt32;

namespace AnotherECS.Views.Core
{
    public interface IViewSystem : ISystem
    {
        uint GetId<T>()
            where T : IViewFactory;
        void Create(EntityId id, uint viewId);
        void Change(EntityId id);
        void Destroy(EntityId id);
    }
}
