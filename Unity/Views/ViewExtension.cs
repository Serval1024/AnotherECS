using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Views.Core;
using EntityId = System.UInt32;

namespace AnotherECS.Views
{
    public static class ViewExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this State state, EntityId id)
            where T : IView
        {
            state.Add(id, new ViewHandle() { ownerId = id, viewId = state.GetConfig<ViewSystemReference>().system.GetId<T>() });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this State state, EntityId id, uint viewId)
        {
            state.Add(id, new ViewHandle() { ownerId = id, viewId = viewId });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this State state, EntityId id)
        {
            state.Remove<ViewHandle>(id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this Entity entity)
            where T : IView
        {
            CreateView<T>(entity.State, entity.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this Entity entity, uint viewId)
        {
            CreateView(entity.State, entity.id, viewId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this Entity entity)
        {
            DestroyView(entity.State, entity.id);
        }
    }
}