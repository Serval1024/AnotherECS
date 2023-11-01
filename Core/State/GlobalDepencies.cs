using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal unsafe struct GlobalDepencies : ISerialize
    {
        public WorldConfig config;
        public TickProvider tickProvider;
        public InjectContainer injectContainer;
        public EntitiesCaller entities;
        public DArrayCaller dArray;
        public ArchetypeCaller archetype;
        public uint componentTypesCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.WriteStruct(config);
            tickProvider.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            config = reader.ReadStruct<WorldConfig>();
            tickProvider.Unpack(ref reader);
        }
    }


    /*
  public unsafe struct FunctionHistory
  {
      public delegate*<void*, void*, void> change;    // void* storage, void* object
      public delegate*<uint, void> revert;            // uint - tick
  }
  */

    /*
    public unsafe struct FunctionStorage
    {
        public delegate*<ref StorageLayout, Type> getType;

        public delegate*<ref StorageLayout, EntityId, IComponent> getCopy;
        public delegate*<ref StorageLayout, EntityId, IComponent, void> set;
    }*/
    /*
        public unsafe struct FunctionStorage<TComponent>
            where TComponent : unmanaged, IComponent
        {
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, ref TComponent> create;

            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, void> init;

            public delegate*<ref StorageLayout<TComponent>, EntityId, ref TComponent> mRead;
            public delegate*<ref StorageLayout<TComponent>, EntityId, ref TComponent> mGet;
            public delegate*<ref StorageLayout<TComponent>, EntityId, ref TComponent, void> mSet;

            public delegate*<ref StorageLayout<TComponent>, EntityId, bool> mIsHas;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, EntityId, ref TComponent, void> mAddData;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, EntityId, ref TComponent> mAdd;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, EntityId, void> mRemove;

            public delegate*<ref StorageLayout<TComponent>, ref TComponent> sRead;
            public delegate*<ref StorageLayout<TComponent>, ref TComponent> sGet;
            public delegate*<ref StorageLayout<TComponent>, ref TComponent, void> sSet;

            public delegate*<ref StorageLayout<TComponent>, bool> sIsHas;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, ref TComponent, void> sAddData;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, ref TComponent> sAdd;
            public delegate*<ref StorageLayout<TComponent>, ref GlobalDepencies, void> sRemove;
        }
    */
}
