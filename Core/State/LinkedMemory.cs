using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    public unsafe struct LinkedMemory<TChunkUnit> : IDisposable, ISerialize, IRevertSetRecycledCountRaw<uint>, IRevertGetRecycledRaw<uint>, IRevertSetCountRaw<uint>, IRevertPtrDenseRaw
        where TChunkUnit : unmanaged
    {
        private readonly ChunkMemory<TChunkUnit> _storage;

#if ANOTHERECS_HISTORY_DISABLE
        public LinkedMemory(uint bufferSize, uint segmentSize, uint recycledCapacity)
#else
        public LinkedMemory(uint bufferSize, uint segmentSize, uint recycledCapacity, in HistoryByChangeArgs args)
#endif
        {
#if ANOTHERECS_HISTORY_DISABLE
            _storage = new ChunkMemory<TChunkUnit>(bufferSize, segmentSize + (uint)sizeof(Chunk), recycledCapacity);
#else
            _storage = new ChunkMemory<TChunkUnit>(bufferSize, segmentSize + (uint)sizeof(Chunk), recycledCapacity, args);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => _storage.GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUpperBoundId()
            => _storage.GetUpperBoundId();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetByteCapacity()
            => _storage.GetByteCapacity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCountCapacity()
            => _storage.GetCountCapacity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add()
            => _storage.Add();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint headId)
        {
            var tail = _storage.Add();
            var chunk = _storage.Read<Chunk>(headId);
            chunk->next = tail;
            return tail;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint UnsafeAdd()
            => _storage.UnsafeAdd();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint UnsafeAdd(uint headId)
        {
            var tail = _storage.UnsafeAdd();
            var chunk = _storage.Read<Chunk>(headId);
            chunk->next = tail;
            return tail;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Read<T>(uint id)
            where T : unmanaged
            => (T*)(byte*)_storage.Read<Chunk>(id) + sizeof(Chunk);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Next(uint id)
            => _storage.Read<Chunk>(id)->next;

        public uint NextOrAdd(uint id)
        {
            var chunk = _storage.Read<Chunk>(id);
            return (chunk->next == 0) 
                ? Add(chunk) 
                : chunk->next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            var chunk = _storage.Read<Chunk>(id);
            _storage.Remove(id);
            while (chunk->next != 0)
            {
                _storage.Remove(chunk->next);
                chunk = _storage.Read<Chunk>(chunk->next);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _storage.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIncSizeDense()
           => _storage.TryIncSizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
            => _storage.TryResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRecycledCountRaw(uint count)
        {
            _storage.SetRecycledCountRaw(count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint[] GetRecycledRaw()
            => _storage.GetRecycledRaw();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCountRaw(uint count)
        {
            _storage.SetCountRaw(count);
        }

        public byte* GetDenseRaw()
           => _storage.GetDenseRaw();

        public void Pack(ref WriterContextSerializer writer)
        {
            _storage.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _storage.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Add(Chunk* chunk)
        {
            var tail = _storage.Add();
            chunk->next = tail;
            return tail;
        }


        private struct Chunk
        {
            public uint next;
        }
    }
}
