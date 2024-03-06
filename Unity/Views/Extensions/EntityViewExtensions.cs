using AnotherECS.Core;
using AnotherECS.Views.Core;
using System.Runtime.CompilerServices;

namespace AnotherECS.Unity.Views
{
    public static class EntityViewExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView<T>(this Entity entity)
            where T : IViewFactory
        {
            entity.State.CreateView<T>(entity.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this Entity entity, ViewId viewId)
        {
            entity.State.CreateView(entity.id, viewId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this Entity entity, ViewGuid viewGuid)
        {
            entity.State.CreateView(entity.id, viewGuid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateView(this Entity entity, string viewGuid)
        {
            entity.State.CreateView(entity.id, viewGuid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView<T>(this Entity entity)
            where T : IViewFactory
        {
            entity.State.ChangeView<T>(entity.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this Entity entity, ViewId viewId)
        {
            entity.State.ChangeView(entity.id, viewId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this Entity entity, ViewGuid viewGuid)
        {
            entity.State.ChangeView(entity.id, viewGuid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeView(this Entity entity, string viewGuid)
        {
            entity.State.ChangeView(entity.id, viewGuid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyView(this Entity entity)
        {
            entity.State.DestroyView(entity.id);
        }
    }
}