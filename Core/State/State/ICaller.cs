using System;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public interface ICaller
    {
        internal unsafe void Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state);
        internal unsafe void AllocateLayout();
        Type GetElementType();
    }

    internal interface ICaller<TComponent> : ICaller
       where TComponent : unmanaged
    {
        TComponent Create();
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

    internal interface IMultiCaller<TComponent> : IMultiCaller, ICaller<TComponent>
        where TComponent : unmanaged, IComponent
    {
        bool IsHas(EntityId id);

        void Add(EntityId id, ref TComponent component);
        ref TComponent Add(EntityId id);

        ref TComponent Read(EntityId id);
        ref TComponent Get(EntityId id);
        ref TComponent Set(EntityId id, ref TComponent component);

    }

    internal interface ISingleCaller<TComponent> : ICaller<TComponent>
        where TComponent : unmanaged, IComponent
    {
        bool IsHas();

        void Add(ref TComponent component);
        ref TComponent Add();
        void SetOrAdd(ref TComponent component);
        void Remove();

        ref TComponent Read();
        ref TComponent Get();
        ref TComponent Set(ref TComponent component);
    }
}

