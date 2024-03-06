using AnotherECS.Core;
using AnotherECS.Views.Core;
using System;

namespace AnotherECS.Unity.Views
{
    public abstract class EntityView : MonoBehaviourView
    {
        public string guid;

        public override ViewGuid GetGUID()
            => new(guid);

        public virtual void OnCreated(ref EntityReadOnly entity) { }
        public virtual void OnApply(ref EntityReadOnly entity) { }
        public virtual void OnDestroyed() { }

        protected override void OnCreatedInternal(ref EntityReadOnly entity)
        {
#if ANOTHERECS_RELEASE
            OnCreated(ref entity);
#else
            var checkCopyEntity = entity;
            OnCreated(ref entity);
            CheckModified(ref checkCopyEntity, ref entity);
#endif
        }

        protected override void OnApplyInternal(ref EntityReadOnly entity)
        {
#if ANOTHERECS_RELEASE
            OnApply(ref entity);
#else
            var checkCopyEntity = entity;
            OnApply(ref entity);
            CheckModified(ref checkCopyEntity, ref entity);
#endif
        }

        protected override void OnDestroyedInternal()
            => OnDestroyed();

        private void CheckModified(ref EntityReadOnly oldEntity, ref EntityReadOnly newEntity)
        {
            if (!newEntity.IsValid || oldEntity != newEntity)
            {
                throw new InvalidOperationException("Entity was modified.");
            }
        }
    }
}
