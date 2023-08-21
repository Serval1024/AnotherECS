using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using UnityEngine.Rendering;
using EntityId = System.Int32;

namespace AnotherECS.Views
{
    [SystemOrder(SystemOrder.First)]
    public class UnityViewSystem : IViewSystem, IConstructModule, ITickFinishiedModule
    {
        private readonly Queue<Command> _commandBuffer;
        private readonly UnityViewController _unityViewController;

        public UnityViewSystem(UnityViewController unityViewController)
        {
            _unityViewController = unityViewController;
            _commandBuffer = new Queue<Command>();
        }

        public void Construct(State state)
        {
            state.SetOrAdd(new ViewSystemReference() { system = this });
        }

        public void TickFinishied(State state)
        {
            while (_commandBuffer.Count != 0)
            {
                var command = _commandBuffer.Dequeue();
                switch (command.type)
                {
                    case Command.Type.Create:
                        Create(state, command.id, command.viewId);
                        break;
                    case Command.Type.Change:
                        Change(command.id);
                        break;
                    case Command.Type.Destroy:
                        Change(command.id);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId<T>()
            where T : IView
            => _unityViewController.GetId<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create<T>(State state, EntityId id)
            where T : IView
            => Create(state, id, _unityViewController.GetId<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create(State state, EntityId id, uint viewId)
            => _unityViewController.CreateView(state, id, viewId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change(EntityId id)
            => _unityViewController.ChangeView(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy(EntityId id)
            => _unityViewController.DestroyView(id);

        private struct Command
        {
            public Type type;
            public EntityId id;
            public uint viewId;

            public enum Type
            {
                Create,
                Change,
                Destroy,
            }
        }
    }
}
