using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout     // Union ComponetLayout and ComponetLayout<TComponent>
    {
        public ComponetStorage storage;
        public HistoryStorage history;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            storage.Clear();
            history.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            storage.Dispose();
            history.Dispose();
        }

        public struct Mock : IComponent { }
    }

    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout<TComponent>     // Union ComponetLayout and ComponetLayout<TComponent>
        where TComponent : unmanaged
    {
        public ComponetStorage storage;
        public HistoryStorage history;
        public ComponentFunction<TComponent> componentFunction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            storage.Clear();
            history.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            storage.Dispose();
            history.Dispose();
        }
    }

    public unsafe struct ComponentFunction<TComponent>
        where TComponent : unmanaged
    {
        internal delegate*<ref GlobalDepencies, ref TComponent, void> construct;
        internal delegate*<ref GlobalDepencies, ref TComponent, void> deconstruct;
    }
  
    public unsafe struct ComponetStorage : IDisposable
    {
        public ArrayPtr sparse;
        public ArrayPtr dense;
        public ArrayPtr version;
        public ArrayPtr recycle;

        public uint recycleIndex;
        public uint denseIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            sparse.Clear();
            dense.Clear();
            version.Clear();
            recycle.Clear();

            recycleIndex = 0;
            denseIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            sparse.Dispose();
            dense.Dispose();
            version.Dispose();
            recycle.Dispose();
        }
    }

    public unsafe struct HistoryStorage : IDisposable
    {
        public ArrayPtr recycleCountBuffer;
        public ArrayPtr recycleBuffer;
        public ArrayPtr countBuffer;
        public ArrayPtr denseBuffer;
        public ArrayPtr sparseBuffer;

        public uint recycleCountIndex;
        public uint recycleIndex;
        public uint countIndex;
        public uint denseIndex;
        public uint sparseIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            recycleCountBuffer.Clear();
            recycleBuffer.Clear();
            countBuffer.Clear();
            denseBuffer.Clear();

            recycleCountIndex = 0;
            recycleIndex = 0;
            countIndex = 0;
            denseIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            recycleCountBuffer.Dispose();
            recycleBuffer.Dispose();
            countBuffer.Dispose();
            denseBuffer.Dispose();
        }
    }

    internal unsafe struct GlobalDepencies : ISerialize
    {
        public GeneralConfig config;
        public TickProvider tickProvider;
        public EntitiesCaller entities;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.WriteStruct(config);
            tickProvider.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            config = reader.ReadStruct<GeneralConfig>();
            tickProvider.Unpack(ref reader);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FastAccess
    {
        public UnmanagedLayout* layoutPtr;
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
