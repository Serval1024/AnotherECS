using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Gen.Common")]
namespace AnotherECS.Core.Caller
{
    public interface ICallerReference { }

    internal interface ICaller : ICallerReference, ISerialize, IRepairMemoryHandle, IRepairStateId
    {
        uint ElementId { get; }
        bool IsSingle { get; }
        bool IsTickFinished { get; }
        bool IsSerialize { get; }
        bool IsResizable { get; }
        bool IsAttach { get; }
        bool IsDetach { get; }
        bool IsInject { get; }
        bool IsTemporary { get; }
        bool IsCallRevertStages { get; }

        uint GetDenseMemoryAllocated { get; }

        internal void AllocateLayout();
        Type GetElementType();

        bool IsHas(EntityId id);

        void Add(EntityId id, IComponent data);
        void Remove(EntityId id);
        void RemoveRaw(EntityId id);

        IComponent GetCopy(EntityId id);
        void Set(EntityId id, IComponent data);

        WArray<uint> ReadVersion();
    }

    internal interface ICaller<TComponent> : ICaller
        where TComponent : unmanaged
    {
        unsafe void Config(Dependencies* dependencies, ushort id, State state, ComponentFunction<TComponent> componentFunction);
        TComponent Create();
        void Add(EntityId id, ref TComponent component);
        ref TComponent Add(EntityId id);
        ref readonly TComponent Read(EntityId id);
        ref TComponent Get(EntityId id);
        void Set(EntityId id, ref TComponent component);
        void SetOrAdd(EntityId id, ref TComponent component);
        uint GetVersion(EntityId id);

        bool TryRead(uint id, out TComponent component);
        bool TryGet(uint id, out TComponent component);

        void Each<TIterator>(ref TIterator iterator)
            where TIterator : struct, IDataIterator<TComponent>;
        IEnumerable<TComponent> GetEnumerable();

        WArray<T> ReadSparse<T>()
           where T : unmanaged;

        WArray<TComponent> ReadDense();
        WArray<TComponent> GetDense();
    }

    public interface IFastAccess
    {
        internal unsafe void Config(ICaller caller);
    }

    internal interface IResizableCaller : ICaller
    {
        void Resize(uint capacity);
    }

    internal interface ITickFinishedCaller
    {
        void TickFinished();
    }

    internal interface IRevertStages
    {
        void RevertStage0();
        void RevertStage1();
        void RevertStage2();
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

