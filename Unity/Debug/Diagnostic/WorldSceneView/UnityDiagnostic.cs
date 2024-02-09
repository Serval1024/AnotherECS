using AnotherECS.Core;
using System.Collections.Generic;
using UnityEngine;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public struct UnityDiagnostic
    {
        private const string _ecsLabel = "AnotherECS";

        private bool _isInit;
        private Transform _root;
        private Dictionary<World, WorldDiagnosticView> _views;


        public void Attach(World world)
        {
            Init();

            if (_root == null)
            {
                _root = new GameObject(_ecsLabel).transform;
            }
            CreateView(world);
        }

        public void Update(World world)
        {
            if (_views.TryGetValue(world, out var view))
            {
                if (view != null)
                {
                    view.UpdateViews();
                }
                else
                {
                    _views.Remove(world);
                }
            }
        }

        public void Detach(World world)
        {
            DestroyView(world);
        }

        private void Init()
        {
            if (!_isInit)
            {
                _isInit = false;
                _views = new();
            }
        }

        private void CreateView(World world)
        {
            var goView = new GameObject();
            goView.transform.parent = _root;

            var view = goView.AddComponent<WorldDiagnosticView>();
            view.Construct(world);
            _views.Add(world, view);
        }

        private void DestroyView(World world)
        {
            if (_views.TryGetValue(world, out var view))
            {
                view.DestroyView();
                _views.Remove(world);
            }

            if (_views.Count == 0)
            {
                if (_root != null)
                {
                    GameObject.Destroy(_root.gameObject);
                }
            }
        }
    }
}
