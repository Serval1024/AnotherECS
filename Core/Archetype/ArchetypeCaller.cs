using System;
using System.Runtime.CompilerServices;
using AnotherECS.Converter;
using AnotherECS.Serializer;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    using NodeCaller = Caller<
               uint, Node, uint, TOData<uint>, uint,
               UintNumber,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               RecycleStorageFeature<uint, Node, uint, TOData<uint>, uint>,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               NonSparseFeature<Node, TOData<uint>, uint>,
               ArchetypeDenseFeature<uint, Node, TOData<uint>>,
               Nothing,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
               Nothing<uint, Node, uint, TOData<uint>, uint>,
#if ANOTHERECS_HISTORY_DISABLE
                Nothing<uint, Node, uint, TOData<uint>, uint>,
#else
                BySegmentHistoryFeature<uint, Node, uint, uint>,
#endif
                BBSerialize<uint, Node, uint, TOData<uint>>,
#if ANOTHERECS_HISTORY_DISABLE
                Nothing<uint, Node, uint, TOData<uint>, uint>
#else
                BySegmentHistoryFeature<uint, Node, uint, uint>
#endif
               >;


    internal unsafe struct ArchetypeCaller : ICaller, IDisposable, ISerialize, IRevertCaller, ITickFinishedCaller, IRevertFinishedCaller
    {
        private NodeCaller _node;
        private ArrayPtr<Node> _archetypes;
        private UnmanagedLayout<uint, Node, uint, TOData<uint>>* _layout;

        public bool IsValide => _node.IsValide;
        public bool IsSingle => false;
        public bool IsRevert => _node.IsRevert;
        public bool IsTickFinished => false;
        public bool IsSerialize => false;
        public bool IsResizable => false;
        public bool IsAttach => false;
        public bool IsDetach => false;
        public bool IsInject => false;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {   
            CallerWrapper.Config<NodeCaller, Node>(ref _node, layout, depencies, id, state);
            _layout = _node.GetLayout();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            CallerWrapper.AllocateLayout<NodeCaller, Node>(ref _node);
            _archetypes = _layout->storage.dense;   //TODO SER BUG
            ArchetypeActions.Setup(ref _archetypes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint archetypeId, uint id, ushort itemId)
            => ArchetypeActions.Add(ref _archetypes, ref _layout->storage.denseIndex, archetypeId, id, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Remove(uint archetypeId, uint id, ushort itemId)
            => ArchetypeActions.Remove(ref _archetypes, ref _layout->storage.denseIndex, archetypeId, id, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint archetypeId, uint id)
        {
            ArchetypeActions.Remove(ref _archetypes, archetypeId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] Filter(ushort[] items)
            => ArchetypeActions.Filter(ref _archetypes, items);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Filter(ushort[] items, int itemCount, uint* result)
            => ArchetypeActions.Filter(ref _archetypes, items, itemCount, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            _node.TickFinished();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            _node.RevertTo(tick, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertFinished()
        {
            _node.RevertFinished();
        }

        public void Dispose()
        {
            _node.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _node.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _node.Unpack(ref reader);
        }


        public Node Create()
        {
            throw new NotImplementedException();
        }

        public Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public void Remove(uint id)
        {
            throw new NotImplementedException();
        }

        public IComponent GetCopy(uint id)
        {
            throw new NotImplementedException();
        }

        public void Set(uint id, IComponent data)
        {
            throw new NotImplementedException();
        }

        public static class LayoutInstaller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ArchetypeCaller Install(State state)
            {
                ArchetypeCaller caller = default;
                caller._node = state.AddLayout<NodeCaller, uint, Node, uint, TIOData<uint>>();
                return caller;
            }
        }
    }

    [IgnoreCompile]
    internal unsafe struct Node : IComponent
    {
        public const int ChildenMax = 16;

        public uint parent;
        public uint archetypeId;
        public ushort itemId;
        public byte childenCount;
        public fixed uint childen[Node.ChildenMax];
        public uint itemsCollectionId;
    }
}