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
            state.CreateView(id, state.GetViewId<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this State state, EntityId id, string viewGuid)
        {
            state.CreateView(id, state.GetViewId(viewGuid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this State state, EntityId id, ViewGuid viewGuid)
        {
            state.CreateView(id, state.GetViewId(viewGuid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this State state, EntityId id, ViewId viewId)
        {
            state.Add(id, new ViewHandle(id, viewId));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView<T>(this State state, EntityId id)
            where T : IViewFactory
        {
            if (state.IsHas<ViewHandle>(id))
            {
                DestroyView(state, id);
            }

            state.CreateView<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this State state, EntityId id, string viewGuid)
        {
            state.ChangeView(id, state.GetViewId(viewGuid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this State state, EntityId id, ViewGuid viewGuid)
        {
            state.ChangeView(id, state.GetViewId(viewGuid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this State state, EntityId id, ViewId viewId)
        {
            if (state.IsHas<ViewHandle>(id))
            {
                DestroyView(state, id);
            }

            state.CreateView(id, viewId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this State state, EntityId id)
        {
            state.Remove<ViewHandle>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ViewId GetViewId<T>(this State state)
            where T : IViewFactory
            => GetId<T>(state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ViewId GetViewId(this State state, string viewGuid)
            => GetId(state, viewGuid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ViewId GetViewId(this State state, ViewGuid viewGuid)
            => GetId(state, viewGuid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ViewId GetId<T>(State state)
            where T : IViewFactory
            => GetIViewSystem(state).GetId<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ViewId GetId(State state, string viewGuid)
            => GetId(state, new ViewGuid(viewGuid));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ViewId GetId(State state, ViewGuid viewGuid)
            => GetIViewSystem(state).GetId(viewGuid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IViewSystem GetIViewSystem(State state)
        {
#if !ANOTHERECS_RELEASE
            Validate(state);
#endif
            return state.GetModuleData<ViewSystemReference>(ViewSystemReference.MODULE_DATA_ID).module;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Validate(State state)
        {
#if !ANOTHERECS_RELEASE
            if (!state.IsHasModuleData(ViewSystemReference.MODULE_DATA_ID))
            {
                throw new Core.Exceptions.FeatureNotExists(nameof(UnityViewModule));
            }
#endif
        }
    }
}