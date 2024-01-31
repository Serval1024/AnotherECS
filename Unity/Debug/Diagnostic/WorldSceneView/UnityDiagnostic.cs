using System.Collections.Generic;
using AnotherECS.Core;
using AnotherECS.Debug.Diagnostic;
using UnityEngine;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public class UnityDiagnostic : IDiagnostic
    {
        private readonly string _ecsLabel = "AnotherECS";

        private Transform _root;
        private readonly Dictionary<World, WorldDiagnosticView> _views = new();


        void IDiagnostic.Attach(World world)
        {
            if (_root == null)
            {
                _root = new GameObject(_ecsLabel).transform;
            }
            CreateView(world);
        }

        void IDiagnostic.Update(World world)
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

        void IDiagnostic.Detach(World world)
        {
            DestroyView(world);
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
