using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class EntitiesActions
    {
        internal const ushort AllocateGeneration = 32768;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateLayout(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies)
        {
            ChunkStorageActions<EntityHead>.AllocateLayout(ref layout, depencies.config.entityCapacity, depencies.config.recycledCapacity);
            MultiStorageActions<EntityHead>.AllocateSparse<uint>(ref layout, 1);

            MultiHistoryFacadeActions<EntityHead>.AllocateRecycle(ref layout, ref depencies);
            MultiHistoryFacadeActions<EntityHead>.AllocateDenseSegment<ushort>(ref layout, ref depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static EntityId Allocate(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies)
        {
            var id = ChunkStorageActions<EntityHead>.UnsafeAdd(ref layout);
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);

            HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, &head->generation);

            head->generation += AllocateGeneration + 1;
            if (head->generation == ushort.MaxValue)
            {
                head->generation = AllocateGeneration;
            }

            head->count = 0;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deallocate(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies, EntityId id)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);

            HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, &head->generation);
            head->generation -= AllocateGeneration;

            var next = head->next;
            var componentsPtr = head->components;
            var count = head->count;
            var iMax = Math.Min(count, EntityHead.ComponentMax);
            for (int i = 0; i < iMax; ++i)
            {
                HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, componentsPtr + i);
            }

            count -= iMax;
            
            ChunkStorageActions<EntityHead>.Remove(ref layout, id);

            if (next != 0)
            {
                var idTail = next;
                var tail = ChunkStorageActions<EntityHead>.Read(ref layout, idTail);
                next = tail->next;

                iMax = Math.Min(count, EntityTail.ComponentMax);
                for (int i = 0; i < iMax; ++i)
                {
                    HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, componentsPtr + i);
                }
                count -= EntityTail.ComponentMax;   //TODO SER CHECK ushort < 0

                ChunkStorageActions<EntityHead>.Remove(ref layout, idTail);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref UnmanagedLayout<EntityHead> layout)
            => MultiStorageActions<EntityHead>.TryResizeDense(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryIncResizeDense(ref UnmanagedLayout<EntityHead> layout)
            => MultiStorageActions<EntityHead>.TryIncResizeDense(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetComponentCount(ref UnmanagedLayout<EntityHead> layout, EntityId id)
            => ChunkStorageActions<EntityHead>.Read(ref layout, id)->count;

        public static ushort GetComponents(ref UnmanagedLayout<EntityHead> layout, EntityId id, ushort[] buffer)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);
            var count = head->count;
#if ANOTHERECS_DEBUG
            if (buffer.Length < count)
            {
                throw new ArgumentException($"There is not enough space in {nameof(buffer)} to copy.");
            }
#endif
            var components = head->components;

            var index = -1;
            for (int i = 0, iMax = Math.Min(count, EntityHead.ComponentMax); i < iMax; ++i)
            {
                buffer[++index] = components[i];
            }

            if (count > EntityHead.ComponentMax)
            {
                var next = head->next;
                EntityTail* tail = null;
                while (next != 0)
                {
                    tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                    components = tail->components;

                    for (int i = 0; i < EntityTail.ComponentMax; ++i)
                    {
                        buffer[++index] = components[i];
                    }

                    next = tail->next;
                }
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetComponent(ref UnmanagedLayout<EntityHead> layout, EntityId id, int index)
        {
            if (index < EntityHead.ComponentMax)
            {
                return ChunkStorageActions<EntityHead>.Read(ref layout, id)->components[index];
            }
            else
            {
                var next = ChunkStorageActions<EntityHead>.Read(ref layout, id)->next;
                index -= EntityHead.ComponentMax;
                while (next != 0)
                {
                    if (index < EntityTail.ComponentMax)
                    {
                        return ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next)->components[index];
                    }

                    next = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next)->next;
                    index -= EntityTail.ComponentMax;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetGeneration(ref UnmanagedLayout<EntityHead> layout, EntityId id)
            => ChunkStorageActions<EntityHead>.Read(ref layout, id)->generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount(ref UnmanagedLayout<EntityHead> layout)
           => MultiStorageActions<EntityHead>.GetCount(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetUpperBoundId(ref UnmanagedLayout<EntityHead> layout)
           => MultiStorageActions<EntityHead>.GetUpperBoundId(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCapacity(ref UnmanagedLayout<EntityHead> layout)
           => MultiStorageActions<EntityHead>.GetCapacity(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies, EntityId id, ushort componentType)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);
            if (head->count < EntityHead.ComponentMax)
            {
                ref var component = ref head->components[head->count];
                component = componentType;
            }
            else
            {
                var next = head->next;
                EntityTail* tail = null;

                if (next == 0)
                {
                    TryIncResizeDense(ref layout);
                    head->next = ChunkStorageActions<EntityHead>.UnsafeAdd(ref layout);
                    tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                    tail->components[0] = componentType;
                }
                else
                {
                    var index = head->count - EntityHead.ComponentMax + EntityTail.ComponentMax;
                    while (next != 0)
                    {
                        tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                        next = tail->next;
                        index -= EntityTail.ComponentMax;
                    }

                    if (index < EntityTail.ComponentMax)
                    {
                        tail->components[index] = componentType;
                    }
                    else
                    {
                        TryIncResizeDense(ref layout);
                        tail->next = ChunkStorageActions<EntityHead>.UnsafeAdd(ref layout);
                        tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                        tail->components[0] = componentType;
                    }
                }
            }

            HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, &head->count);

            ++head->count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies, EntityId id, ushort componentType)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);

            var componentPtr = FindComponentPtr(ref layout, id, componentType);
            if (componentPtr != null)
            {
                var lastPtr = FindComponentLastPtr(ref layout, id);

                if (componentPtr != lastPtr)
                {
                    HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, componentPtr);

                    *componentPtr = *lastPtr;
                }

                HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, &head->count);
                
                --head->count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint GetCurrentIndexGC(ref UnmanagedLayout<EntityHead> layout)
            => ref layout.storage.sparse.GetRef<uint>(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCurrentIndexGC(ref UnmanagedLayout<EntityHead> layout, uint value)
            => layout.storage.sparse.Set(0, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InterationGarbageCollect(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies, int checkCount)
        {
            ref var currentIndexGC = ref GetCurrentIndexGC(ref layout);

            var count = MultiStorageActions<EntityHead>.GetUpperBoundId(ref layout);

            while (--checkCount > 0)
            {
                if (++currentIndexGC == count)
                {
                    currentIndexGC = 0;
                }

                var entity = ChunkStorageActions<EntityHead>.Read(ref layout, currentIndexGC);
                if (entity->generation >= AllocateGeneration && entity->count == 0)
                {
                    DeallocateZero(ref layout, ref depencies, currentIndexGC);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeallocateZero(ref UnmanagedLayout<EntityHead> layout, ref GlobalDepencies depencies, EntityId id)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);

            HistoryActions<EntityHead>.PushSegment(ref layout, depencies.tickProvider.tick, depencies.config.history.recordTickLength, &head->generation);
            
            head->generation -= AllocateGeneration;
            ChunkStorageActions<EntityHead>.Remove(ref layout, id);
        }

        private static ushort* FindComponentPtr(ref UnmanagedLayout<EntityHead> layout, EntityId id, ushort componentType)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);
            var components = head->components;

            for (int i = 0, iMax = Math.Min(head->count, EntityHead.ComponentMax); i < iMax; ++i)
            {
                if (components[i] == componentType)
                {
                    return components + i;
                }
            }

            var next = head->next;
            EntityTail* tail = null;
            while (next != 0)
            {
                tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                components = tail->components;

                for (int i = 0; i < EntityTail.ComponentMax; ++i)
                {
                    if (components[i] == componentType)
                    {
                        return components + i;
                    }
                }

                next = tail->next;
            }
            return null;
        }

        private static ushort* FindComponentLastPtr(ref UnmanagedLayout<EntityHead> layout, EntityId id)
        {
            var head = ChunkStorageActions<EntityHead>.Read(ref layout, id);

            if (head->count <= EntityHead.ComponentMax)
            {
                return head->components + head->count - 1;
            }
            else
            {
                var index = head->count - EntityHead.ComponentMax + EntityTail.ComponentMax;
                var next = head->next;
                EntityTail* tail = null;
                while (next != 0)
                {
                    tail = ChunkStorageActions<EntityHead>.ReadAs<EntityTail>(ref layout, next);
                    next = tail->next;
                }
                return tail->components + index;
            }
        }

    }
}

