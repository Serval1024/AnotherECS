using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct IdCollection<TAllocator> : ISerialize, IDisposable, IEnumerable<uint>
        where TAllocator : unmanaged, IAllocator
    {
        private NHashSetZero<TAllocator, uint, U4U4HashProvider> _data;

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IdCollection(TAllocator* allocator, uint capacity)
        {
            _data = new NHashSetZero<TAllocator, uint, U4U4HashProvider>(allocator, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id)
        {
            _data.Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            _data.Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSetZero<TAllocator, uint, U4U4HashProvider>.Enumerator GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => _data.GetEnumerator();
    }
}