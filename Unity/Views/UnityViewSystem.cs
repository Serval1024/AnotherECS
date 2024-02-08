using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Views.Core;
using EntityId = System.UInt32;

namespace AnotherECS.Views
{
    [SystemOrder(SystemOrder.Last)]
    public class UnityViewSystem : IViewSystem, IMainThread, ICreateModule, ITickFinishedModule
    {
        private readonly ConcurrentQueue<Command> _commandBuffer;
        private readonly UnityViewController _unityViewController;

        public UnityViewSystem(UnityViewController unityViewController)
        {
            _unityViewController = unityViewController;
            _commandBuffer = new ConcurrentQueue<Command>();
        }

        public void OnCreateModule(State state)
        {
            state.SetOrAddConfig(new ViewSystemReference() { system = this });
        }

        public void OnTickFinished(State state)
        {
            while (!_commandBuffer.IsEmpty)
            {
                if (_commandBuffer.TryDequeue(out Command command))
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId<T>()
            where T : IView
            => _unityViewController.GetId<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create(State state, EntityId id, uint viewId)
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
        private void CreateInternal(State state, EntityId id, uint viewId)
            => _unityViewController.CreateView(state, id, viewId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeInternal(EntityId id)
            => _unityViewController.ChangeView(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DestroyInternal(EntityId id)
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
