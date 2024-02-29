using AnotherECS.Core;
using AnotherECS.Views.Core;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AnotherECS.Unity.Views
{
    public abstract class MonoBehaviourView : MonoBehaviour, IView, IViewFactory
    {
        private State _state;
        private Entity _entity;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, ISingle
            => ref _state.Read<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetConfig<T>()
            where T : IConfig
            => _state.GetConfig<T>();

        void IView.Construct(State state, in Entity entity)
        {
            _state = state;
            _entity = entity;
        }

        void IView.Destroyed()
        {
            OnDestroyed();
            Destroy(gameObject);
        }

        string IViewFactory.GetGUID()
            => GetType().Name;

        IView IViewFactory.Create()
            => Instantiate(this);

        void IView.Created()
            => OnCreated(ref _entity);

        void IView.Apply()
            => OnApply(ref _entity);

        public virtual void OnCreated(ref Entity entity) { }
        public virtual void OnApply(ref Entity entity) { }
        public virtual void OnDestroyed() { }
    }
}
