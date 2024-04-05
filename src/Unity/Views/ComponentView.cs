using AnotherECS.Core;

namespace AnotherECS.Unity.Views
{
    public abstract class ComponentView : MonoBehaviourView
    {
        protected override void OnCreatedInternal(ref EntityReadOnly entity) { }
        protected override void OnDestroyedInternal() { }
    }

    public abstract class ComponentView<TComponent0> : ComponentView
        where TComponent0 : unmanaged, IComponent
    {
        public abstract void OnApply(in TComponent0 component);

        protected override void OnApplyInternal(ref EntityReadOnly entity)
        {
            if (entity.TryRead<TComponent0>(out var component))
            {
                OnApply(component);
            }
        }
    }

    public abstract class ComponentView<TComponent0, TComponent1> : ComponentView
        where TComponent0 : unmanaged, IComponent
        where TComponent1 : unmanaged, IComponent
    {
        public abstract void OnApply(in TComponent0 component0, in TComponent1 component1);

        protected override void OnApplyInternal(ref EntityReadOnly entity)
        {
            if (entity.TryRead<TComponent0>(out var component0) &&
                entity.TryRead<TComponent1>(out var component1)
                )
            {
                OnApply(component0, component1);
            }
        }
    }

    public abstract class ComponentView<TComponent0, TComponent1, TComponent2> : ComponentView
        where TComponent0 : unmanaged, IComponent
        where TComponent1 : unmanaged, IComponent
        where TComponent2 : unmanaged, IComponent
    {
        public abstract void OnApply(in TComponent0 component0, in TComponent1 component1, in TComponent2 component2);

        protected override void OnApplyInternal(ref EntityReadOnly entity)
        {
            if (entity.TryRead<TComponent0>(out var component0) &&
                entity.TryRead<TComponent1>(out var component1) &&
                entity.TryRead<TComponent2>(out var component2)
                )
            {
                OnApply(component0, component1, component2);
            }
        }
    }

    public abstract class ComponentView<TComponent0, TComponent1, TComponent2, TComponent3> : ComponentView
        where TComponent0 : unmanaged, IComponent
        where TComponent1 : unmanaged, IComponent
        where TComponent2 : unmanaged, IComponent
        where TComponent3 : unmanaged, IComponent
    {
        public abstract void OnApply(in TComponent0 component0, in TComponent1 component1, in TComponent2 component2, in TComponent3 component3);

        protected override void OnApplyInternal(ref EntityReadOnly entity)
        {
            if (entity.TryRead<TComponent0>(out var component0) &&
                entity.TryRead<TComponent1>(out var component1) &&
                entity.TryRead<TComponent2>(out var component2) &&
                entity.TryRead<TComponent3>(out var component3)
                )
            {
                OnApply(component0, component1, component2, component3);
            }
        }
    }
}
