using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Converter;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout : ISerialize     // Union ComponetLayout and ComponetLayout<TComponent>
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

        public void Pack(ref WriterContextSerializer writer)
        {
            storage.Pack(ref writer);
            history.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            storage.Unpack(ref reader);
            history.Unpack(ref reader);
        }

        [IgnoreCompile]
        public struct Mock : IComponent { }
    }

    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout<TComponent> : ISerialize     // Union ComponetLayout and ComponetLayout<TComponent>
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

        public void Pack(ref WriterContextSerializer writer)
        {
            storage.Pack(ref writer);
            history.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            storage.Unpack(ref reader);
            history.Unpack(ref reader);
        }
    }

    public unsafe struct ComponentFunction<TComponent>
        where TComponent : unmanaged
    {
        public delegate*<ref InjectContainer, ref TComponent, void> construct;
        public delegate*<ref InjectContainer, ref TComponent, void> deconstruct;
    }
  
    public unsafe struct ComponetStorage : IDisposable, ISerialize
    {
        public ArrayPtr sparse;
        public ArrayPtr dense;
        public ArrayPtr version;
        public ArrayPtr recycle;

        public uint denseIndex;
        public uint recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            sparse.Clear();
            dense.Clear();
            version.Clear();
            recycle.Clear();

            denseIndex = 0;
            recycleIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            sparse.Dispose();
            dense.Dispose();
            version.Dispose();
            recycle.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackCommon(ref WriterContextSerializer writer)
        {
            sparse.Pack(ref writer);
            version.Pack(ref writer);
            recycle.Pack(ref writer);

            writer.Write(denseIndex);
            writer.Write(recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackDense(ref WriterContextSerializer writer)
        {
            dense.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            PackCommon(ref writer);
            PackDense(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackCommon(ref ReaderContextSerializer reader)
        {
            sparse.Unpack(ref reader);
            version.Unpack(ref reader);
            recycle.Unpack(ref reader);

            denseIndex = reader.ReadUInt32();
            recycleIndex = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackDense(ref ReaderContextSerializer reader)
        {
            dense.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            UnpackCommon(ref reader);
            UnpackDense(ref reader);
        }
    }

    public unsafe struct HistoryStorage : IDisposable, ISerialize
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
            sparseBuffer.Clear();

            recycleCountIndex = 0;
            recycleIndex = 0;
            countIndex = 0;
            denseIndex = 0;
            sparseIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            recycleCountBuffer.Dispose();
            recycleBuffer.Dispose();
            countBuffer.Dispose();
            denseBuffer.Dispose();
            sparseBuffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackCommon(ref WriterContextSerializer writer)
        {
            recycleCountBuffer.Pack(ref writer);
            recycleBuffer.Pack(ref writer);
            countBuffer.Pack(ref writer);
            sparseBuffer.Pack(ref writer);

            writer.Write(recycleCountIndex);
            writer.Write(recycleIndex);
            writer.Write(countIndex);
            writer.Write(denseIndex);
            writer.Write(sparseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackDense(ref WriterContextSerializer writer)
        {
            denseBuffer.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            PackCommon(ref writer);
            PackDense(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackCommon(ref ReaderContextSerializer reader)
        {
            recycleCountBuffer.Unpack(ref reader);
            recycleBuffer.Unpack(ref reader);
            countBuffer.Unpack(ref reader);
            sparseBuffer.Unpack(ref reader);

            recycleCountIndex = reader.ReadUInt32();
            recycleIndex = reader.ReadUInt32();
            countIndex = reader.ReadUInt32();
            denseIndex = reader.ReadUInt32();
            sparseIndex = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackDense(ref ReaderContextSerializer reader)
        {
            denseBuffer.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            UnpackCommon(ref reader);
            UnpackDense(ref reader);
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
