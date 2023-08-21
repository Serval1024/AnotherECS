using AnotherECS.Core;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AnotherECS.Views
{
    public abstract class MonoBehaviourView : MonoBehaviour, IView
    {
        private State _state;
        private Entity _entity;

        public void Construct(State state, in Entity entity)
        {
            _state = state;
            _entity = entity;
        }

        public string GetGUID()
            => GetType().Name;

        public void Send(BaseEvent @event)
            => _state.Send(@event);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>()
            where T : struct, IShared
            => _state.Read<T>();

        public void Destroyed()
        {
            OnDestroying();
            Destroy(gameObject);
        }

        public IView Create()
          => Instantiate(this);

        public void Created()
            => OnCreated(ref _entity);

        public void Apply()
            => OnApply(ref _entity);

        public abstract void OnApply(ref Entity entity);
        public abstract void OnCreated(ref Entity entity);
        public abstract void OnDestroying();
    }
}
