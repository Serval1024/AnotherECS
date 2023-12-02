using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 208)]   //208
    public unsafe struct UnmanagedLayout     // Union UnmanagedLayout and UnmanagedLayout<,,,>
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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponetStorage : IDisposable
    {
        public NArray sparse;
        public NArray dense;
        public NArray<uint> version;
        public NArray<uint> recycle;

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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HistoryStorage : IDisposable
    {
        public NArray<TData<uint>> recycleCountBuffer;
        public NArray<TOData<uint>> recycleBuffer;
        public NArray<TData<uint>> countBuffer;
        public NArray denseBuffer;
        public NArray sparseBuffer;
        public NArray<uint> versionIndexer;

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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponentFunction<TDense>
        where TDense : unmanaged
    {
        public delegate*<ref InjectContainer, ref TDense, void> construct;
        public delegate*<ref InjectContainer, ref TDense, void> deconstruct;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponetStorage<TSparse, TDense, TDenseIndex> : IDisposable
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public NArray<TSparse> sparse;
        public NArray<TDense> dense;
        public NArray<uint> version;
        public NArray<TDenseIndex> recycle;

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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HistoryStorage<TSparse, TDenseIndex, WTickDataDense> : IDisposable
        where TSparse: unmanaged
        where TDenseIndex : unmanaged
        where WTickDataDense : unmanaged
    {
        public NArray<TData<uint>> recycleCountBuffer;
        public NArray<TOData<TDenseIndex>> recycleBuffer;
        public NArray<TData<uint>> countBuffer;
        public NArray<WTickDataDense> denseBuffer;
        public NArray<TOData<TSparse>> sparseBuffer;
        public NArray<uint> versionIndexer;

        public uint recycleCountIndex;
        public uint recycleIndex;
        public uint countIndex;
        public uint denseIndex;
        public uint sparseIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (recycleCountBuffer.IsValide)
            {
                recycleCountBuffer.Clear();
            }
            if (recycleBuffer.IsValide)
            {
                recycleBuffer.Clear();
            }
            if (countBuffer.IsValide)
            {
                countBuffer.Clear();
            }
            if (denseBuffer.IsValide)
            {
                denseBuffer.Clear();
            }
            if (sparseBuffer.IsValide)
            {
                sparseBuffer.Clear();
            }
            if (versionIndexer.IsValide)
            {
                versionIndexer.Clear();
            }

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
