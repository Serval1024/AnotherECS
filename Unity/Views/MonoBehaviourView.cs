using AnotherECS.Core;
using AnotherECS.Views.Core;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AnotherECS.Unity.Views
{
    public interface IMonoBehaviourView : IView, IViewFactory { }

    public abstract class MonoBehaviourView : MonoBehaviour, IMonoBehaviourView
    {
        private State _state;
        private EntityReadOnly _entity;

        private bool _isSingle;
        private MonoBehaviourView[] _all;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, ISingle
            => ref _state.Read<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetConfig<T>()
            where T : IConfig
            => _state.GetConfig<T>();

        void IView.Construct(State state, in EntityReadOnly entity)
        {
            if (_all == null)
            {
                _all ??= GetComponents<MonoBehaviourView>();
                _isSingle = _all.Length == 1;
            }

            if (_isSingle)
            {
                ConstructInternal(state, in entity);
            }
            else
            {
                for (int i = 0; i < _all.Length; ++i)
                {
                    _all[i].ConstructInternal(state, in entity);
                }
            }
        }

        private void ConstructInternal(State state, in EntityReadOnly entity)
        {
            _state = state;
            _entity = entity;
        }

        public virtual ViewGuid GetGUID()
            => default;

        IView IViewFactory.Create()
        {
            var inst =  Instantiate(this);
            var view = inst.GetComponent<MonoBehaviourView>();
            view._all = _all;
            view._isSingle = _isSingle;

            return inst;
        }

        void IView.Created()
        {
            if (_isSingle)
            {
                OnCreatedInternal(ref _entity);
            }
            else
            {
                for(int i = 0; i < _all.Length; ++i)
                {
                    _all[i].OnCreatedInternal(ref _entity);
                }
            }
        }

        void IView.Apply()
        {
            if (_isSingle)
            {
                OnApplyInternal(ref _entity);
            }
            else
            {
                for (int i = 0; i < _all.Length; ++i)
                {
                    _all[i].OnApplyInternal(ref _entity);
                }
            }
        }

        void IView.Destroyed()
        {
            if (_isSingle)
            {
                OnDestroyedInternal();
            }
            else
            {
                for (int i = 0; i < _all.Length; ++i)
                {
                    _all[i].OnDestroyedInternal();
                }
            }
        }

        protected abstract void OnCreatedInternal(ref EntityReadOnly entity);
        protected abstract void OnApplyInternal(ref EntityReadOnly entity);
        protected abstract void OnDestroyedInternal();
    }
}
