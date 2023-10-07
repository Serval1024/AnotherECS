using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 208)]
    public unsafe struct UnmanagedLayout     // Union ComponetLayout and ComponetLayout<TComponent> 192
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

    [StructLayout(LayoutKind.Sequential, Size = 208)]
    public unsafe struct UnmanagedLayout<QSparse, WDense>     // Union ComponetLayout and ComponetLayout<TComponent>
        where QSparse : unmanaged
        where WDense : unmanaged
    {
        public ComponetStorage<QSparse, WDense> storage;
        public HistoryStorage history;
        public ComponentFunction<WDense> componentFunction;

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
  
    public unsafe struct ComponetStorage<QSparse, WDense> : IDisposable
        where QSparse : unmanaged
        where WDense : unmanaged
    {
        public ArrayPtr<QSparse> sparse;
        public ArrayPtr<WDense> dense;
        public ArrayPtr<uint> version;
        public ArrayPtr<uint> recycle;

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
        public ArrayPtr versionIndexer;

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
            versionIndexer.Clear();

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
            versionIndexer.Dispose();
        }
    }
}
