using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AnotherECS.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ULayout<TAllocator, TSparse, TDense, TDenseIndex> : IDisposable, IRepairMemoryHandle
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
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            if (sparse.IsValid)
            {
                RepairMemoryCaller.Repair(ref sparse, ref repairMemoryContext);
            }
            if (dense.IsValid)
            {
                RepairMemoryCaller.Repair(ref dense, ref repairMemoryContext);
            }
            if (recycle.IsValid)
            {
                RepairMemoryCaller.Repair(ref recycle, ref repairMemoryContext);
            }
            if (tickVersion.IsValid)
            {
                RepairMemoryCaller.Repair(ref tickVersion, ref repairMemoryContext);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComponentFunction<TDense>
       where TDense : unmanaged
    {
        public delegate*<ref InjectContainer, ref TDense, void> construct;
        public delegate*<ref InjectContainer, ref TDense, void> deconstruct;
        public delegate*<ref RepairMemoryContext, ref TDense, void> repairMemory;
        public delegate*<ushort, ref TDense, void> repairStateId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GenerationULayout<TAllocator> : IDisposable, IRepairMemoryHandle
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
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            if (generation.IsValid)
            {
                RepairMemoryCaller.Repair(ref generation, ref repairMemoryContext);
            }
        }
    }
}
