using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Gen.Common")]
namespace AnotherECS.Core
{
    public interface ICaller
    {
        internal unsafe void Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state);
        internal unsafe void AllocateLayout();
        Type GetElementType();
    }

    public interface IFastAccess<TCaller>
        where TCaller : ICaller
    {
        internal unsafe void Config(TCaller caller);
    }

    internal interface ICaller<TComponent> : ICaller
       where TComponent : unmanaged
    {
        TComponent Create();

        public bool IRevert { get; }
        public bool IsTickFinished { get; }
        public bool IsSerialize { get; }
        public bool IsResizable { get; }
    }

    internal interface IResizableCaller : ICaller
    {
        void Resize(uint capacity);
    }

    internal interface IMultiCaller : ICaller
    {
        IComponent GetCopy(EntityId id);
        void Set(EntityId id, IComponent component);
        void Remove(EntityId id);
    }

    internal interface IMultiCaller<TComponent> : IMultiCaller, ICaller<TComponent>, IResizableCaller
        where TComponent : unmanaged, IComponent
    {
        bool IsHas(EntityId id);

        void Add(EntityId id, ref TComponent component);
        ref TComponent Add(EntityId id);

        ref readonly TComponent Read(EntityId id);
        ref TComponent Get(EntityId id);
        void Set(EntityId id, ref TComponent component);

    }

    internal interface ISingleCaller<TComponent> : ICaller<TComponent>
        where TComponent : unmanaged, IComponent
    {
        bool IsHas();

        void Add(ref TComponent component);
        ref TComponent Add();
        void SetOrAdd(ref TComponent component);
        void Remove();

        ref readonly TComponent Read();
        ref TComponent Get();
        void Set(ref TComponent component);
    }

    internal interface IAttachCaller
    {
        void Attach();
    }

    internal interface IDetachCaller
    {
        void Detach();
    }
}

