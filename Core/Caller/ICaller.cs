﻿using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Gen.Common")]
namespace AnotherECS.Core.Caller
{
    public interface ICallerReference { }

    internal interface ICaller : ICallerReference, ISerialize, IRebindMemoryHandle
    {
        ushort ElementId { get; }
        bool IsSingle { get; }
        bool IsTickFinished { get; }
        bool IsSerialize { get; }
        bool IsResizable { get; }
        bool IsAttach { get; }
        bool IsDetach { get; }
        bool IsInject { get; }
        bool IsTemporary { get; }
        internal unsafe void Config(void* layout, GlobalDepencies* depencies, ushort id, DirtyHandler<HAllocator> dirtyHandler, State state);
        internal void AllocateLayout();
        Type GetElementType();
        void Remove(EntityId id);
        void RemoveRaw(EntityId id);
        IComponent GetCopy(EntityId id);
        void Set(EntityId id, IComponent data);
    }

    public interface IFastAccess
    {
        internal unsafe void Config(ICaller caller);
    }

    internal interface ICaller<TComponent> : ICaller
        where TComponent : unmanaged
    {
        TComponent Create();
        bool IsHas(EntityId id);
        void Add(EntityId id, ref TComponent component);
        ref TComponent Add(EntityId id);
        ref readonly TComponent Read(EntityId id);
        ref TComponent Get(EntityId id);
        void Set(EntityId id, ref TComponent component);
        void SetOrAdd(EntityId id, ref TComponent component);
    }

    internal interface IResizableCaller : ICaller
    {
        void Resize(uint capacity);
    }

    internal interface ITickFinishedCaller
    {
        void TickFinished();
    }

    internal interface IRevertFinishedCaller
    {
        void RevertFinished();
    }

    internal interface IAttachCaller
    {
        void Attach();
    }

    internal interface IDetachCaller
    {
        void Detach();
    }

    public interface IInjectCaller
    {
        void CallConstruct();
        void CallDeconstruct();
    }
}
