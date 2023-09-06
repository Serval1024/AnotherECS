using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe class Entities : ISerializeConstructor, IDisposable
    {
        internal const ushort AllocateGeneration = 32768;

        private ChunkMemory<ushort> _storage;

        private int _gcEntityCheckPerTick;
        private uint _currentIndexGC;


        internal Entities(ref ReaderContextSerializer reader, in EntitiesArgs args)
        {
            Unpack(ref reader, args);
        }

        public Entities(in EntitiesArgs args)
        {
            var size = (uint)sizeof(EntityHead);
#if ANOTHERECS_HISTORY_DISABLE
            _storage = new ChunkMemory<ushort>(size * args.entityCapacity, size, args.recycledCapacity);
#else
            _storage = new ChunkMemory<ushort>(size * args.entityCapacity, size, args.recycledCapacity, args.history);
#endif
            _gcEntityCheckPerTick = (int)args.gcEntityCheckPerTick;
        }
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id <= GetCount() && IsHasRaw(id);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => GetGeneration(id) >= AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponentCount(EntityId id)
            => _storage.Read<EntityHead>(id)->count;

        public ushort GetComponents(EntityId id, ushort[] buffer)
        {
            var head = _storage.Read<EntityHead>(id);
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
                    tail = _storage.Read<EntityTail>(next);
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

        public ushort GetComponent(EntityId id, int index)
        {
            if (index < EntityHead.ComponentMax)
            {
                return _storage.Read<EntityHead>(id)->components[index];
            }
            else
            {
                var next = _storage.Read<EntityHead>(id)->next;
                index -= EntityHead.ComponentMax;
                while(next != 0)
                {
                    if (index < EntityTail.ComponentMax)
                    {
                        return _storage.Read<EntityTail>(next)->components[index];
                    }    

                    next = _storage.Read<EntityTail>(next)->next;
                    index -= EntityTail.ComponentMax;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetGeneration(EntityId id)
            => _storage.Read<EntityHead>(id)->generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => _storage.GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => _storage.GetCountCapacity();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ushort componentType)
        {
            var head = _storage.Read<EntityHead>(id);
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
                    _storage.TryIncSizeDense();
                    head->next = _storage.UnsafeAdd();
                    tail = _storage.Read<EntityTail>(next);
                    tail->components[0] = componentType;
                }
                else
                {
                    var index = head->count - EntityHead.ComponentMax + EntityTail.ComponentMax;
                    while (next != 0)
                    {
                        tail = _storage.Read<EntityTail>(next);
                        next = tail->next;
                        index -= EntityTail.ComponentMax;
                    }

                    if (index < EntityTail.ComponentMax)
                    {
                        tail->components[index] = componentType;
                    }
                    else
                    {
                        _storage.TryIncSizeDense();
                        tail->next = _storage.UnsafeAdd();
                        tail = _storage.Read<EntityTail>(next);
                        tail->components[0] = componentType;
                    }
                }
            }

#if !ANOTHERECS_HISTORY_DISABLE
            _storage.Change(&head->count);
#endif
            ++head->count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id, ushort componentType)
        {
            var head = _storage.Read<EntityHead>(id);

            var componentPtr = FindComponentPtr(id, componentType);
            if (componentPtr != null)
            {
                var lastPtr = FindComponentLastPtr(id);

                if (componentPtr != lastPtr)
                {
#if !ANOTHERECS_HISTORY_DISABLE
                    _storage.Change(componentPtr);
#endif
                    *componentPtr = *lastPtr;
                }
#if !ANOTHERECS_HISTORY_DISABLE
                _storage.Change(&head->count);
#endif
                --head->count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResize()
            => _storage.TryResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate()
        {
            var id = _storage.UnsafeAdd();
            var head = _storage.Read<EntityHead>(id);

            _storage.Change(&head->generation);

            head->generation += AllocateGeneration + 1;
            if (head->generation == ushort.MaxValue)
            {
                head->generation = AllocateGeneration;
            }

            head->count = 0;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
        {
            var head = _storage.Read<EntityHead>(id);

            _storage.Change(&head->generation);
            head->generation -= AllocateGeneration;

            var next = head->next;

#if !ANOTHERECS_HISTORY_DISABLE
            var components = head->components;
            var count = head->count;
            var iMax = Math.Min(count, EntityHead.ComponentMax);
            for (int i = 0; i < iMax; ++i)
            {
                _storage.Change(components + i);
            }

            count -= iMax;
#endif
            _storage.Remove(id);


            if (next != 0)
            {
                var idTail = next;
                var tail = _storage.Read<EntityTail>(idTail);
                next = tail->next;

#if !ANOTHERECS_HISTORY_DISABLE
                iMax = Math.Min(count, EntityTail.ComponentMax);
                for (int i = 0; i < iMax; ++i)
                {
                    _storage.Change(components + i);
                }
                count -= EntityTail.ComponentMax;   //TODO SER CHECK ushort < 0
#endif
                _storage.Remove(idTail);
            }
        }

        public void TickFinished()
        {
            if (_gcEntityCheckPerTick != 0)
            {
                InterationGarbageCollect(_gcEntityCheckPerTick);
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _storage.Pack(ref writer);
            writer.Write(_gcEntityCheckPerTick);
            writer.Write(_currentIndexGC);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader, default);
        }

        public void Unpack(ref ReaderContextSerializer reader, in EntitiesArgs args)
        {
            _storage.Unpack(ref reader, args.history);
            _gcEntityCheckPerTick = reader.ReadInt32();
            _currentIndexGC = reader.ReadUInt32();
        }

        public void Dispose()
        {
            _storage.Dispose();
        }


        private ushort* FindComponentPtr(EntityId id, ushort componentType)
        {
            var head = _storage.Read<EntityHead>(id);
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
                tail = _storage.Read<EntityTail>(next);
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

        private ushort* FindComponentLastPtr(EntityId id)
        {
            var head = _storage.Read<EntityHead>(id);

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
                    tail = _storage.Read<EntityTail>(next);
                    next = tail->next;
                }
                return tail->components + index;
            }
        }

        private void InterationGarbageCollect(int checkCount)
        {
            var count = _storage.GetUpperBoundId();
            
            while (--checkCount > 0)
            {
                if (++_currentIndexGC == count)
                {
                    _currentIndexGC = 0;
                }

                var entity = _storage.Read<EntityHead>(_currentIndexGC);
                if (entity->generation >= AllocateGeneration && entity->count == 0)
                {
                    DeallocateZero(_currentIndexGC);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeallocateZero(EntityId id)
        {
            var head = _storage.Read<EntityHead>(id);

            _storage.Change(&head->generation);
            head->generation -= AllocateGeneration;
            _storage.Remove(id);
        }
    }
}
