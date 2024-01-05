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
            if (sparse.IsValid)
            {
                sparse.Clear();
            }
            if (dense.IsValid)
            {
                dense.Clear();
            }
            if (recycle.IsValid)
            {
                recycle.Clear();
            }
            if (tickVersion.IsValid)
            {
                tickVersion.Clear();
            }
            if (addRemoveVersion.IsValid)
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
            if (sparse.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref sparse, ref rebinder);
            }
            if (dense.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref dense, ref rebinder);
            }
            if (recycle.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref recycle, ref rebinder);
            }
            if (tickVersion.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref tickVersion, ref rebinder);
            }
            if (addRemoveVersion.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref addRemoveVersion, ref rebinder);
            }
        }
    }
}
