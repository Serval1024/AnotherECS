using AnotherECS.Core.Threading;
using AnotherECS.Views.Core;
using EntityId = System.UInt32;

namespace AnotherECS.Views
{
    public static class ViewExtension
    {
        public static ThreadRestrictionsBuilder UseView(ref this ThreadRestrictionsBuilder builder)
            => builder.Use<ViewHandle>();

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this State context, EntityId id)
            where T : IView
        {
            //context.Add(id, new ViewHandle() { ownerId = id, viewId = context.Get<ViewSystemReference>().system.GetId<T>() });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this State context, EntityId id, uint viewId)
        {
            context.Add(id, new ViewHandle() { ownerId = id, viewId = viewId });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this State context, EntityId id)
        {
            context.Remove<ViewHandle>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this Entity entity)
            where T : IView
        {
            CreateView<T>(entity.state, entity.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this Entity entity, uint viewId)
        {
            CreateView(entity.state, entity.id, viewId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this Entity entity)
        {
            DestroyView(entity.state, entity.id);
        }*/
    }
}