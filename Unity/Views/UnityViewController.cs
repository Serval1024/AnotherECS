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

        private Config _config;
        private readonly Dictionary<EntityId, IView> _byIdInstances = new();

        private void Awake()
        {
            _config = new(views);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateView<T>(State state, EntityId id)
            where T : IViewFactory
            => CreateView(state, id, _config.Get<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateView(State state, EntityId id, uint viewId)
            => CreateView(state, id, _config.Get(viewId));

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
        public uint GetId<T>()
            where T : IViewFactory
            => _config.GetId<T>();


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

        
        public class Config
        {
            private readonly IViewFactory[] _byIds;
            private readonly Dictionary<Type, uint> _byTypeToIds;
            private readonly Dictionary<string, IViewFactory> _byGUIDs;
            private readonly Dictionary<Type, IViewFactory> _byTypes;

            public Config(IEnumerable<IViewFactory> registredViews)
            {
                if (registredViews.Any(p => p == null))
                {
                    throw new NullReferenceException();
                }

                _byIds = registredViews.OrderBy(p => p.GetGUID()).ToArray();
                _byGUIDs = registredViews.ToDictionary(k => k.GetGUID(), v => v);
                _byTypes = registredViews.ToDictionary(k => k.GetType(), v => v);
                _byTypeToIds = _byIds.Select((p, i) => (p, i)).ToDictionary(k => k.p.GetType(), v => (uint)v.i);
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
            public IViewFactory Get(string id)
            {
#if !ANOTHERECS_RELEASE
                if (!_byGUIDs.ContainsKey(id))
                {
                    throw new Exceptions.ViewNotFoundException(id);
                }
#endif
                return _byGUIDs[id];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IViewFactory Get(uint id)
            {
#if !ANOTHERECS_RELEASE
                if (id >= _byIds.Length)
                {
                    throw new Exceptions.ViewNotFoundException(id.ToString());
                }
#endif
                return _byIds[id];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetId<T>()
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
        }
    }
}
