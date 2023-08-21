using AnotherECS.Serializer;
using System.Runtime.CompilerServices;
using EntityId = System.Int32;

[assembly: InternalsVisibleTo("AnotherECS.Gen.Common")]
[assembly: InternalsVisibleTo("AnotherECS.Views")]
namespace AnotherECS.Core
{
    public interface IAdapterReference
    {
    }

    [Serialize]
    internal interface IAdapter : IAdapterReference, ISerialize
    {
        void Rebind(IPool pool);
        IPool GetPool();
        void Clear();
#if ANOTHERECS_DEBUG
        void SetState(IDebugException state);
#endif
    }

    internal unsafe interface IEntityAdapter : IAdapter
    {
        void BindExternal(Entities entities, Filters filters, ref Adapters adapters);
        void Resize(int capacity);
        bool IsHas(EntityId id);
        void AddSyncVoid(EntityId id, State state, delegate*<State, int, void> sync);
        bool RemoveRaw(EntityId id);
        bool RemoveSync(EntityId id);
        IComponent GetCopy(EntityId id);
        void SetUnknow(EntityId id, IComponent component);
    }

    internal unsafe interface IEntityAdapterAdd<T> : IAdapter
        where T : struct, IComponent
    {
        ref T AddSync(EntityId id, State state, delegate*<State, int, void> sync);
        void AddSyncVoid(EntityId id, ref T data, State state, delegate*<State, int, void> sync);
    }

    internal interface IEntityAdapter<T> : IEntityAdapter, IEntityAdapterAdd<T>
        where T : struct, IComponent
    {
        ref readonly T Read(EntityId id);
        ref T Get(EntityId id);
        void Set(EntityId id, ref T data);
    }

    internal interface ISingleAdapter : IAdapter
    {
        bool IsHas();
        void AddSyncVoid();
        void RemoveSync();
    }

    internal interface ISingleAdapter<T> : ISingleAdapter
        where T : struct, IComponent
    {
        ref readonly T Read();
        ref T Get();
        public void Set(ref T data);
        public void SetOrAdd(ref T data);
        ref T AddSync();
    }

    internal interface IDisposableInternal
    {
        void Dispose();
    }

    internal interface IRecycleInternal
    {
        void Recycle();
    }

    internal interface IAttachInternal
    {
        void Attach();
    }

    internal interface IDetachInternal
    {
        void Detach();
    }

    internal interface IHistoryBindExternalInternal
    {
        void BindExternal(IHistory history);
    }

    internal interface IStateBindExternalInternal
    {
        void BindExternal(State state);
    }



    internal interface IInjectSupportInternal
    {
        void BindInject(ref InjectContainer injectContainer, IInjectMethodsReference[] injectMethods);
        void ReInject();
    }
}

