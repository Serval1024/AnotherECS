using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ULayout<TAllocator, TSparse, TDense, TDenseIndex> : IDisposable, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where TSparse : unmanaged
        where TDense : unmanaged
        where TDenseIndex : unmanaged
    {
        public NArray<TAllocator, TSparse> sparse;
        public NArray<TAllocator, TDense> dense;
        public NArray<TAllocator, TDenseIndex> recycle;
        public NArray<TAllocator, uint> tickVersion;


        public uint denseIndex;
        public uint recycleIndex;

        public static uint GetSize()
            => (uint)sizeof(ULayout<TAllocator, TSparse, TDense, TDenseIndex>);

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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            sparse.Dispose();
            dense.Dispose();
            recycle.Dispose();
            tickVersion.Dispose();
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
    public unsafe struct GenerationULayout<TAllocator> : IDisposable, IRebindMemoryHandle
       where TAllocator : unmanaged, IAllocator
    {
        public NArray<TAllocator, byte> generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            generation.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            generation.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            if (generation.IsValid)
            {
                MemoryRebinderCaller.Rebind(ref generation, ref rebinder);
            }
        }
    }
}
