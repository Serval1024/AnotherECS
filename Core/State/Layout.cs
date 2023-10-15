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

    public unsafe struct ComponetStorage : IDisposable
    {
        public ArrayPtr sparse;
        public ArrayPtr dense;
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
        public ArrayPtr<TickData<uint>> recycleCountBuffer;
        public ArrayPtr<TickOffsetData<uint>> recycleBuffer;
        public ArrayPtr<TickData<uint>> countBuffer;
        public ArrayPtr denseBuffer;
        public ArrayPtr sparseBuffer;
        public ArrayPtr<uint> versionIndexer;

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


    [StructLayout(LayoutKind.Sequential, Size = 208)]
    public unsafe struct UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>     // Union ComponetLayout and ComponetLayout<TComponent>
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
        where TTickData : unmanaged
    {
        public ComponetStorage<TSparse, TDense, TDenseIndex> storage;
        public HistoryStorage<TSparse, TDenseIndex, TTickData> history;
        public ComponentFunction<TDense> componentFunction;

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

    public unsafe struct ComponentFunction<TDense>
        where TDense : unmanaged
    {
        public delegate*<ref InjectContainer, ref TDense, void> construct;
        public delegate*<ref InjectContainer, ref TDense, void> deconstruct;
    }
  
    public unsafe struct ComponetStorage<TSparse, TDense, TDenseIndex> : IDisposable
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public ArrayPtr<TSparse> sparse;
        public ArrayPtr<TDense> dense;
        public ArrayPtr<uint> version;
        public ArrayPtr<TDenseIndex> recycle;

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

    public unsafe struct HistoryStorage<TSparse, TDenseIndex, WTickDataDense> : IDisposable
        where TSparse: unmanaged
        where TDenseIndex : unmanaged
        where WTickDataDense : unmanaged
    {
        public ArrayPtr<TickData<uint>> recycleCountBuffer;
        public ArrayPtr<TickOffsetData<TDenseIndex>> recycleBuffer;
        public ArrayPtr<TickData<uint>> countBuffer;
        public ArrayPtr<WTickDataDense> denseBuffer;
        public ArrayPtr<TickOffsetData<TSparse>> sparseBuffer;
        public ArrayPtr<uint> versionIndexer;

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
