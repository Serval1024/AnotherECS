using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Core.Collection;
using AnotherECS.Exceptions;
using AnotherECS.Serializer;

namespace AnotherECS.Collections
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [ForceBlittable]
    public unsafe struct DHashSet<T> : IInject<WPtr<AllocatorSelector>>, IEnumerable<T>, ISerialize, IRepairMemoryHandle
        where T : unmanaged, IEquatable<T>
    {
        private NHashSet<AllocatorSelector, T, HashProvider> _data;

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        internal DHashSet(AllocatorSelector* allocator)
        {
            _data = default;
            _data.SetAllocator(allocator);
#if !ANOTHERECS_RELEASE
            Validate();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<WPtr<AllocatorSelector>>.Construct(
            [InjectMap(nameof(BAllocator), "allocatorType=1")]
            [InjectMap(nameof(HAllocator), "allocatorType=2")]
            WPtr<AllocatorSelector> allocator)
        {
            _data.SetAllocator(allocator.Value);
#if !ANOTHERECS_RELEASE
            Validate();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        {
            Deallocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _data, ref repairMemoryContext);
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }

        public void Allocate(uint length)
        {
            if (!_data.IsAllocatorValid())
            {
                throw new MissInjectException(typeof(DArray<T>));
            }

            _data.Allocate(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _data.Dispose();
        }

        public bool Contains(T item)
            => _data.Contains(item);

        public void Add(T item)
            => _data.Add(item);

        public bool Remove(T item)
            => _data.Remove(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
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
        public Enumerator GetEnumerator()
          => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();


#if !ANOTHERECS_RELEASE
        private void Validate()
        {
            if (!ComponentUtils.IsSimple(typeof(T)))
            {
                throw new DArraySimpleException(typeof(T));
            }
        }
#endif

        private struct HashProvider : IHashProvider<T, uint>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetHash(ref T key)
                => (uint)key.GetHashCode();
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private DHashSet<T> _data;
            private NHashSet<AllocatorSelector, T, HashProvider>.Enumerator _enumerator;
            private int _index;
            private T _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref DHashSet<T> data)
            {
                _data = data;
                _enumerator = data._data.GetEnumerator();
                _index = 0;
                _current = default;

                _data.EnterCheckChanges();
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.IsValid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => _enumerator.MoveNext();

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            object IEnumerator.Current
            {
                get => _enumerator.Current;
            }

            void IEnumerator.Reset()
            {
                CallReset(ref _enumerator);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CallReset<TEnumerator>(ref TEnumerator enumerator)
                where TEnumerator : struct, IEnumerator
            {
                enumerator.Reset();
            }
        }
    }
}
