using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 232)]
    public unsafe struct UnmanagedLayout
    {
        public static uint GetSize()
            => (uint)sizeof(UnmanagedLayout);
    }

    [StructLayout(LayoutKind.Sequential, Size = 232)]
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
        public NArray<TAllocator, TDenseIndex> recycle;
        public NArray<TAllocator, uint> tickVersion;
        public NArray<TAllocator, byte> addRemoveVersion;

        public uint denseIndex;
        public uint recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (sparse.IsValide)
            {
                sparse.Clear();
            }
            if (dense.IsValide)
            {
                dense.Clear();
            }
            if (recycle.IsValide)
            {
                recycle.Clear();
            }
            if (tickVersion.IsValide)
            {
                tickVersion.Clear();
            }
            if (addRemoveVersion.IsValide)
            {
                addRemoveVersion.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            sparse.Dispose();
            dense.Dispose();
            recycle.Dispose();
            tickVersion.Dispose();
            addRemoveVersion.Dispose();
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
            if (recycle.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref recycle, ref rebinder);
            }
            if (tickVersion.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref tickVersion, ref rebinder);
            }
            if (addRemoveVersion.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref addRemoveVersion, ref rebinder);
            }
        }
    }
}
