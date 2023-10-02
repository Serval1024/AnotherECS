using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

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
        public delegate*<ref InjectContainer, ref TComponent, void> construct;
        public delegate*<ref InjectContainer, ref TComponent, void> deconstruct;
    }
  
    public unsafe struct ComponetStorage : IDisposable
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
    }
}
