using AnotherECS.Core;
using AnotherECS.Views.Core;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AnotherECS.Views
{
    public abstract class MonoBehaviourView : MonoBehaviour, IView
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

        string IView.GetGUID()
            => GetType().Name;

        void IView.Destroyed()
        {
            OnDestroying();
            Destroy(gameObject);
        }

        IView IView.Create()
            => Instantiate(this);

        void IView.Created()
            => OnCreated(ref _entity);

        void IView.Apply()
            => OnApply(ref _entity);

        public abstract void OnApply(ref Entity entity);
        public virtual void OnCreated(ref Entity entity) { }
        public virtual void OnDestroying() { }
    }
}
