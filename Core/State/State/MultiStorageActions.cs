using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class StorageActions<T>
        where T : unmanaged
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallConstruct(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ref T component)
        {
            layout.componentFunction.construct(ref depencies, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallDeconstruct(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ref T component)
        {
            layout.componentFunction.deconstruct(ref depencies, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StorageClear(ref UnmanagedLayout<T> layout)
        {
            ref var storage = ref layout.storage;

            storage.sparse.Clear();
            storage.dense.Clear();
            storage.version.Clear();
        }
    }

    internal static unsafe class SingleStorageActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallConstruct_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            if (*sparse)
            {
                layout.componentFunction.construct(ref depencies, ref *dense);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount(ref UnmanagedLayout<T> layout)
         => layout.storage.sparse.Get<bool>(0) ? (uint)1 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHas_bool(ref UnmanagedLayout<T> layout)
            => *layout.storage.sparse.GetPtr<bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ref T component)
        {
            MultiHistoryFacadeActions<T>.PushDense(ref layout, ref depencies, 0, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            *layout.storage.version.GetPtr<uint>() = depencies.tickProvider.tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSparse_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();

            *sparse = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparse_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            *sparse = true;

            return ref *dense;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSparseHistory_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, 0, false);
            *sparse = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparseHistory_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, 0, false);
            *sparse = true;

            return ref *dense;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSparse_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            *sparse = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSparse_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            *sparse = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSparseHistory_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, 0, true);
            *sparse = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSparseHistory_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, 0, true);
            *sparse = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ReadDirect(ref UnmanagedLayout<T> layout)
            => ref *layout.storage.dense.GetPtr<T>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallDeconstruct_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            ref var deconstruct = ref layout.componentFunction.deconstruct;

            if (*sparse)
            {
                layout.componentFunction.deconstruct(ref depencies, ref *dense);
            }
        }
    }

    internal static unsafe class MultiStorageActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateDense(ref UnmanagedLayout<T> layout, uint denseCapacity)
        {
            ref var storage = ref layout.storage;
            storage.dense = ArrayPtr.Create<T>(denseCapacity);
            storage.denseIndex = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateSparse<USparse>(ref UnmanagedLayout<T> layout, uint capacity)
            where USparse : unmanaged
        {
            layout.storage.sparse = ArrayPtr.Create<USparse>(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateRecycle(ref UnmanagedLayout<T> layout, uint recycledCapacity)
        {
            layout.storage.recycle = ArrayPtr.Create<ushort>(recycledCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateVersion(ref UnmanagedLayout<T> layout, uint denseCapacity)
        {
            layout.storage.version = ArrayPtr.Create<uint>(denseCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallConstruct_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();
            var denseIndex = storage.denseIndex;

            ref var construct = ref layout.componentFunction.construct;

            for (uint i = 1; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    construct(ref depencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallConstruct_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<byte>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                ref var construct = ref layout.componentFunction.construct;

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        construct(ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallConstruct_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<ushort>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                ref var construct = ref layout.componentFunction.construct;

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        construct(ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallDeconstruct_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();
            var denseIndex = storage.denseIndex;

            ref var deconstruct = ref layout.componentFunction.deconstruct;

            for (uint i = 1; i < denseIndex; ++i)
            {
                if (sparse[i])
                {
                    deconstruct(ref depencies, ref dense[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallDeconstruct_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<byte>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                ref var deconstruct = ref layout.componentFunction.deconstruct;

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        deconstruct(ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CallDeconstruct_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint count)
        {
            if (count != 0)
            {
                ref var storage = ref layout.storage;

                var sparse = storage.sparse.GetPtr<ushort>();
                var sparseLength = storage.sparse.ElementCount;
                var dense = storage.dense.GetPtr<T>();

                ref var deconstruct = ref layout.componentFunction.deconstruct;

                for (uint i = 1; i < sparseLength; ++i)
                {
                    if (sparse[i] != 0)
                    {
                        deconstruct(ref depencies, ref dense[sparse[i]]);
                        if (--count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount(ref UnmanagedLayout<T> layout)
           => GetUpperBoundId(ref layout) - 1 - GetRecycleCount(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetUpperBoundId(ref UnmanagedLayout<T> layout)
           => layout.storage.denseIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetRecycleCount(ref UnmanagedLayout<T> layout)
           => layout.storage.recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCapacity(ref UnmanagedLayout<T> layout)
            => layout.storage.dense.ElementCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref UnmanagedLayout<T> layout)
           => TryResizeDense(ref layout, layout.storage.dense.ElementCount << 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncResizeDense(ref UnmanagedLayout<T> layout)
            => TryResizeDense(ref layout, layout.storage.dense.ElementCount + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref UnmanagedLayout<T> layout, uint size)
        {
            ref var storage = ref layout.storage;
            if (storage.denseIndex == storage.dense.ElementCount)
            {
                layout.storage.dense.Resize<T>(size);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDenseVersion(ref UnmanagedLayout<T> layout)
           => TryResizeDenseVersion(ref layout, layout.storage.dense.ElementCount << 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncResizeDenseVersion(ref UnmanagedLayout<T> layout)
            => TryResizeDenseVersion(ref layout, layout.storage.dense.ElementCount + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDenseVersion(ref UnmanagedLayout<T> layout, uint size)
        {
            ref var storage = ref layout.storage;
            if (storage.denseIndex == storage.dense.ElementCount)
            {
                layout.storage.dense.Resize<T>(size);
                layout.storage.version.Resize<uint>(size);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeVersion(ref UnmanagedLayout<T> layout)
          => TryResizeVersion(ref layout, layout.storage.dense.ElementCount << 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncResizeVersion(ref UnmanagedLayout<T> layout)
            => TryResizeVersion(ref layout, layout.storage.dense.ElementCount + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeVersion(ref UnmanagedLayout<T> layout, uint size)
        {
            ref var storage = ref layout.storage;
            if (storage.denseIndex == storage.dense.ElementCount)
            {
                layout.storage.version.Resize<uint>(size);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeRecycle(ref UnmanagedLayout<T> layout)
            => TryResizeRecycle(ref layout, layout.storage.recycle.ElementCount << 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeRecycle(ref UnmanagedLayout<T> layout, uint size)
        {
            ref var storage = ref layout.storage;
            if (storage.recycleIndex == storage.recycle.ElementCount)
            {
                layout.storage.recycle.Resize<ushort>(size);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AllocateIdHistory(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;

            if (recycleIndex > 0)
            {
                MultiHistoryFacadeActions<T>.PushRecycledCount(ref layout, ref depencies, recycleIndex);
                return storage.recycle.Get<ushort>(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
#if ANOTHERECS_DEBUG
                CheckDenseLimit<ushort>(ref layout);
#endif
                MultiHistoryFacadeActions<T>.PushCount(ref layout, ref depencies, denseIndex);
                return denseIndex++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AllocateIdIncrementHistory(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var denseIndex = ref layout.storage.denseIndex;
#if ANOTHERECS_DEBUG
            CheckDenseLimit<ushort>(ref layout);
#endif
            MultiHistoryFacadeActions<T>.PushCount(ref layout, ref depencies, denseIndex);
            return denseIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AllocateId(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;

            if (recycleIndex > 0)
            {
                return storage.recycle.Get<ushort>(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
#if ANOTHERECS_DEBUG
                CheckDenseLimit<ushort>(ref layout);
#endif
                return denseIndex++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AllocateIdIncrement(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var denseIndex = ref layout.storage.denseIndex;
#if ANOTHERECS_DEBUG
            CheckDenseLimit<ushort>(ref layout);
#endif
            return denseIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckDenseLimit<UNumber>(ref UnmanagedLayout<T> layout)
            where UNumber : unmanaged
        {
            if (layout.storage.denseIndex == GetMaxValue<UNumber>())
            {
                throw new Exceptions.ReachedLimitComponentException(GetMaxValue<UNumber>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHas_bool(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.sparse.GetPtr<bool>()[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHas_byte(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.sparse.GetPtr<byte>()[id] != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHas_ushort(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.sparse.GetPtr<ushort>()[id] != 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSparse_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();

            sparse[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparse_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            sparse[id] = true;

            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparse_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, byte denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<byte>();
            var dense = storage.dense.GetPtr<T>();

            sparse[id] = denseIndex;
            
            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparse_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<ushort>();
            var dense = storage.dense.GetPtr<T>();

            sparse[id] = denseIndex;

            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSparseHistory_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, false);
            sparse[id] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparseHistory_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<bool>();
            var dense = storage.dense.GetPtr<T>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, false);
            sparse[id] = true;

            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparseHistory_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, byte denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<byte>();
            var dense = storage.dense.GetPtr<T>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = denseIndex;

            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SetSparseHistory_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id, ushort denseIndex)
        {
            ref var storage = ref layout.storage;

            var sparse = storage.sparse.GetPtr<ushort>();
            var dense = storage.dense.GetPtr<T>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = denseIndex;

            return ref dense[denseIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveDense(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint denseIndex, ref T component)
        {
            MultiHistoryFacadeActions<T>.PushDense(ref layout, ref depencies, denseIndex, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ushort denseIndex)
        {
            layout.storage.version.GetPtr<uint>()[denseIndex] = depencies.tickProvider.tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateVersion(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies)
        {
            var tick = depencies.tickProvider.tick;
            var version = layout.storage.version.GetPtr<uint>();
            for (uint i = 0, iMax = layout.storage.version.ElementCount; i < iMax; ++i)
            {
                version[i] = tick;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeDense(ref UnmanagedLayout<T> layout, uint size)
        {
            layout.storage.dense.Resize<T>(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeVersion(ref UnmanagedLayout<T> layout, uint size)
        {
            layout.storage.dense.Resize<uint>(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeSparse<USparse>(ref UnmanagedLayout<T> layout, uint size)
            where USparse : unmanaged
        {
            layout.storage.sparse.Resize<USparse>(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeallocateId(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ushort denseIndex)
        {
            TryResizeRecycle(ref layout);

            ref var recycleIndex = ref layout.storage.recycleIndex;
            var recycle = layout.storage.recycle.GetPtr<ushort>();
            recycle[recycleIndex++] = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeallocateIdHistory(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, ushort denseIndex)
        {
            TryResizeRecycle(ref layout);

            ref var recycleIndex = ref layout.storage.recycleIndex;
            var recycle = layout.storage.recycle.GetPtr<ushort>();

            MultiHistoryFacadeActions<T>.PushRecycle(ref layout, ref depencies, recycleIndex, recycle[recycleIndex]);
            MultiHistoryFacadeActions<T>.PushCount(ref layout, ref depencies, recycleIndex);

            recycle[recycleIndex++] = denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RemoveSparse_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            sparse[id] = false;
            return (ushort)id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RemoveSparse_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            sparse[id] = false;
            return (ushort)id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RemoveSparse_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<byte>();
            var denseIndex = sparse[id];

            sparse[id] = 0;
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RemoveSparse_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<ushort>();
            var denseIndex = sparse[id];

            sparse[id] = 0;
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RemoveSparseHistory_empty(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, true);
            sparse[id] = false;
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RemoveSparseHistory_bool(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<bool>();

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, true);
            sparse[id] = false;
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RemoveSparseHistory_byte(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<byte>();
            var denseIndex = sparse[id];

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = 0;
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RemoveSparseHistory_ushort(ref UnmanagedLayout<T> layout, ref GlobalDepencies depencies, uint id)
        {
            var sparse = layout.storage.sparse.GetPtr<ushort>();
            var denseIndex = sparse[id];

            MultiHistoryFacadeActions<T>.PushSparse(ref layout, ref depencies, id, sparse[id]);
            sparse[id] = 0;
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetDense_bool(ref UnmanagedLayout<T> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetDense_byte(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.sparse.GetPtr<byte>()[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetDense_ushort(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.sparse.GetPtr<ushort>()[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Read(ref UnmanagedLayout<T> layout, uint id)
        {
            ref var storage = ref layout.storage;
            return ref storage.dense.GetPtr<T>()[storage.sparse.GetPtr<byte>()[id]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ReadDirect(ref UnmanagedLayout<T> layout, byte denseIndex)
            => ref layout.storage.dense.GetPtr<T>()[denseIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ReadDirect(ref UnmanagedLayout<T> layout, ushort denseIndex)
            => ref layout.storage.dense.GetPtr<T>()[denseIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ReadDirect(ref UnmanagedLayout<T> layout, uint denseIndex)
            => ref layout.storage.dense.GetPtr<T>()[denseIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetVersion_bool(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.version.GetPtr<uint>()[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetVersion_byte(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.version.GetPtr<uint>()[GetDense_byte(ref layout, id)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetVersion_ushort(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.version.GetPtr<uint>()[GetDense_ushort(ref layout, id)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StorageClear(ref UnmanagedLayout<T> layout)
        {
            ref var storage = ref layout.storage;
            storage.sparse.Clear();
            storage.dense.Clear(storage.denseIndex);
            storage.version.Clear(storage.denseIndex);
            storage.recycle.Clear(storage.recycleIndex);

            layout.storage.denseIndex = 1;
        }

        public static uint GetMaxValue<UNumber>()
            where UNumber : unmanaged
            => Type.GetTypeCode(typeof(UNumber)) switch
            {
                TypeCode.Byte => byte.MaxValue,
                TypeCode.UInt16 => ushort.MaxValue,
                TypeCode.UInt32 => uint.MaxValue,
                _ => throw new ArgumentException(),
            };
    }


}

