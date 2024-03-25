using AnotherECS.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public class WorldDiagnosticView : MonoBehaviour
    {
        private readonly string _worldLabel = "Another world";
        internal World World { get; private set; }

        internal VisualData visualData;

        private readonly List<EntityId> _destroyTemp = new();
        private EntityDiagnosticView[] _views = Array.Empty<EntityDiagnosticView>();


        internal void Construct(World world)
        {
            World = world;
            gameObject.name = string.IsNullOrEmpty(world.Name) ? $"{_worldLabel}: <No name>" : $"{_worldLabel}: '{world.Name}'";
        }

        internal void UpdateViews()
        {
            if (!World.IsDisposed)
            {
                var idMax = World.State.GetEntityIdMax();
                if (_views.Length < idMax)
                {
                    Array.Resize(ref _views, (int)idMax);
                }

                for (int i = 1; i < _views.Length; ++i)
                {
                    if (_views[i] != null)
                    {
                        _views[i].UpdateView();

                        if (!_views[i].IsValid)
                        {
                            _destroyTemp.Add((EntityId)i);
                        }
                    }
                    else
                    {
                        var id = (EntityId)i;
                        if (World.State.IsHas(id))
                        {
                            CreateView(id);
                        }
                    }
                }

                if (_destroyTemp.Count != 0)
                {
                    foreach (var index in _destroyTemp)
                    {
                        DestroyView(index);
                    }

                    _destroyTemp.Clear();
                }

                visualData.Update(World);
            }
            else
            {
                DestroyView();
            }
        }

        internal void DestroyView()
        {
            if (this != null)
            {
                foreach (var view in _views)
                {
                    if (view != null)
                    {
                        view.DestroyView();
                    }
                }
                _views = Array.Empty<EntityDiagnosticView>();
                Destroy(gameObject);
            }
        }

        private void CreateView(EntityId id)
        {
            var go = new GameObject();
            go.transform.parent = transform;
            var view = go.AddComponent<EntityDiagnosticView>();
            view.Construct(this, id);
            view.UpdateView();
            _views[(int)id] = view;
        }

        private void DestroyView(EntityId id)
        {
            var view = _views[(int)id];
            view.DestroyView();
            _views[(int)id] = null;
        }


        internal struct VisualData
        {
            public WorldStatisticData data;

            public void Update(World world)
            {
                data = world.GetStatistic();
            }
        }
    }
}
