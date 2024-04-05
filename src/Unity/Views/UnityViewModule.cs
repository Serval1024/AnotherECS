using AnotherECS.Core;
using AnotherECS.Views.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Views
{
    [SystemOrder(SystemOrder.Last)]
    public class UnityViewModule : IModule, IViewSystem, IMainThread, ITickFinishedModule, IAttachToStateModule, IDetachToStateModule
    {
        private readonly ConcurrentQueue<Command> _commandBuffer;
        private readonly UnityViewController _unityViewController;

        public UnityViewModule(IEnumerable<MonoBehaviourView> prefabViews)
            : this(CreateUnityViewController(prefabViews)) { }

        public UnityViewModule(UnityViewController unityViewController)
        {
            _unityViewController = unityViewController;
            _commandBuffer = new ConcurrentQueue<Command>();
        }


        public void OnAttachToStateModule(State state)
        {
            state.SetModuleData(ViewSystemReference.MODULE_DATA_ID, new ViewSystemReference() { module = this });
        }

        public void OnDetachToStateModule(State state)
        {
            state.SetModuleData(ViewSystemReference.MODULE_DATA_ID, (ViewSystemReference)null);
        }

        public void OnTickFinished(State state)
        {
            while (_commandBuffer.TryDequeue(out Command command))
            {
                switch (command.type)
                {
                    case Command.Type.Create:
                        CreateInternal(state, command.id, command.viewId);
                        break;
                    case Command.Type.Change:
                        ChangeInternal(command.id);
                        break;
                    case Command.Type.Destroy:
                        DestroyInternal(command.id);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ViewId GetId<T>()
            where T : IViewFactory
            => _unityViewController.GetId<T>();

        public ViewId GetId(ViewGuid guid)
            => _unityViewController.GetId(guid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create(EntityId id, ViewId viewId)
        {
            _commandBuffer.Enqueue(new Command { type = Command.Type.Create, id = id, viewId = viewId });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(EntityId id)
        {
            _commandBuffer.Enqueue(new Command { type = Command.Type.Change, id = id });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy(EntityId id)
        {
            _commandBuffer.Enqueue(new Command { type = Command.Type.Destroy, id = id });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateInternal(State state, EntityId id, ViewId viewId)
            => _unityViewController.CreateView(state, id, viewId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeInternal(EntityId id)
            => _unityViewController.ChangeView(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DestroyInternal(EntityId id)
            => _unityViewController.DestroyView(id);

        private static UnityViewController CreateUnityViewController(IEnumerable<MonoBehaviourView> views)
        {
            if (views == null)
            {
                throw new NullReferenceException(nameof(views));
            }
            var go = new GameObject(nameof(UnityViewModule));
            var unityViewController = go.AddComponent<UnityViewController>();
            unityViewController.views = views.ToList();
            return unityViewController;
        }

        private struct Command
        {
            public Type type;
            public EntityId id;
            public ViewId viewId;

            public enum Type
            {
                Create,
                Change,
                Destroy,
            }
        }
    }
}
