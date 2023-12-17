using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout
    {
        public static uint GetSize()
            => (uint)sizeof(UnmanagedLayout);
    }

    [StructLayout(LayoutKind.Sequential, Size = 192)]
    public unsafe struct UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> : IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public ComponetStorage<TAllocator, TSparse, TDense, TDenseIndex> storage;
        public ComponentFunction<TDense> componentFunction;

        public static uint GetSize()
            => (uint)sizeof(UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            storage.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            storage.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref storage, ref rebinder);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponentFunction<TDense>
        where TDense : unmanaged
    {
        public delegate*<ref InjectContainer, ref TDense, void> construct;
        public delegate*<ref InjectContainer, ref TDense, void> deconstruct;
        public delegate*<ref MemoryRebinderContext, ref TDense, void> memoryRebind;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponetStorage<TAllocator, TSparse, TDense, TDenseIndex> : IDisposable, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public NArray<TAllocator, TSparse> sparse;
        public NArray<TAllocator, TDense> dense;
        public NArray<TAllocator, uint> version;
        public NArray<TAllocator, TDenseIndex> recycle;

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
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            if (sparse.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref sparse, ref rebinder);
            }
            if (dense.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref dense, ref rebinder);
            }
            if (version.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref version, ref rebinder);
            }
            if (recycle.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref recycle, ref rebinder);
            }
        }
    }
}