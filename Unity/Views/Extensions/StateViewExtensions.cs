using AnotherECS.Core;
using AnotherECS.Views.Core;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Views
{
    public static class StateViewExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this State state, EntityId id)
            where T : IViewFactory
        {
#if !ANOTHERECS_RELEASE
            if (!state.IsHasConfig<ViewSystemReference>())
            {
                throw new Core.Exceptions.FeatureNotExists(nameof(UnityViewModule));
            }
#endif
            state.Add(id, new ViewHandle() { ownerId = id, viewId = state.GetConfig<ViewSystemReference>().module.GetId<T>() });
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
            where T : IViewFactory
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