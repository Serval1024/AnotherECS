using AnotherECS.Core;
using AnotherECS.Views.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Views
{
    public class UnityViewController : MonoBehaviour
    {
        public List<MonoBehaviourView> views;

        private Data _data;
        private readonly Dictionary<EntityId, IView> _byIdInstances = new();

        private void Awake()
        {
            _data = new(views);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateView<T>(State state, EntityId id)
            where T : IViewFactory
            => CreateView(state, id, _data.Get<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateView(State state, EntityId id, ViewId viewId)
            => CreateView(state, id, _data.Get(viewId));

        public void CreateView(State state, EntityId id, IViewFactory factory)
        {
            if (_byIdInstances.TryGetValue(id, out IView instance))
            {
                DestroyView(id, instance);
            }
            var inst = factory.Create();
            _byIdInstances.Add(id, inst);
            inst.Construct(state, EntityExtensions.ToEntity(state, id).ToReadOnly());
            inst.Created();
        }

        public void ChangeView(EntityId id)
        {
            if (_byIdInstances.TryGetValue(id, out IView instance))
            {
                instance.Apply();
            }
        }

        public void DestroyView(EntityId id)
        {
            if (_byIdInstances.TryGetValue(id, out IView instance))
            {
                DestroyView(id, instance);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ViewId GetId<T>()
            where T : IViewFactory
            => _data.GetId<T>();

        public ViewId GetId(ViewGuid guid)
            => _data.GetId(guid);

        private void Update()
        {
            foreach(var inst in _byIdInstances.Values)
            {
                inst.Apply();
            }
        }

        private void DestroyView(EntityId id, IView view)
        {
            _byIdInstances.Remove(id);
            view.Destroyed();
        }

        
        private class Data
        {
            private readonly IViewFactory[] _byIds;
            private readonly Dictionary<Type, ViewId> _byTypeToIds;
            private readonly Dictionary<ViewGuid, ViewId> _byGUIDToIds;
            private readonly Dictionary<Type, IViewFactory> _byTypes;

            public Data(IEnumerable<IViewFactory> registeredViews)
            {
                if (registeredViews == null || registeredViews.Any(p => p == null))
                {
                    throw new NullReferenceException();
                }

                _byIds = registeredViews.OrderBy(p =>
                {
                    var guid = p.GetGUID();
                    return guid.IsValid ? guid.ToString() : p.GetType().Name;
                }
                    ).ToArray();

                _byGUIDToIds = new Dictionary<ViewGuid, ViewId>();
                _byTypes = new Dictionary<Type, IViewFactory>();
                _byTypeToIds = new Dictionary<Type, ViewId>();

                for(uint i = 0; i < _byIds.Length; ++i)
                {
                    var viewId = new ViewId(i);
                    var guid = _byIds[i].GetGUID();
                    if (guid.IsValid)
                    {
                        _byGUIDToIds.Add(guid, viewId);
                    }
                    var view = _byIds[i];
                    var type = view.GetType();
                    if (!_byTypes.ContainsKey(type))
                    {
                        _byTypes.Add(type, view);
                        _byTypeToIds.Add(type, viewId);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IViewFactory Get<T>()
                where T : IViewFactory
            {
                var id = typeof(T);
#if !ANOTHERECS_RELEASE
                if (!_byTypes.ContainsKey(id))
                {
                    throw new Exceptions.ViewNotFoundException(id.Name);
                }
#endif
                return _byTypes[id];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IViewFactory Get(ViewId id)
            {
#if !ANOTHERECS_RELEASE
                if (id.ToNumber() >= _byIds.Length)
                {
                    throw new Exceptions.ViewNotFoundException(id.ToString());
                }
#endif
                return _byIds[id.ToNumber()];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ViewId GetId<T>()
                where T : IViewFactory
            {
                var id = typeof(T);
#if !ANOTHERECS_RELEASE
                if (!_byTypeToIds.ContainsKey(id))
                {
                    throw new Exceptions.ViewNotFoundException(id.Name);
                }
#endif
                return _byTypeToIds[id];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ViewId GetId(ViewGuid guid)
            {
                return _byGUIDToIds[guid];
            }
        }
    }
}