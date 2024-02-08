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
    public unsafe struct DHashSet<TValue> : IInject<WPtr<AllocatorSelector>>, IEnumerable<TValue>, ICollection, IValid, ISerialize, IRepairMemoryHandle
        where TValue : unmanaged, IEquatable<TValue>
    {
        private NHashSet<AllocatorSelector, TValue, HashProvider> _data;

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
                throw new MissInjectException(typeof(DHashSet<TValue>));
            }

            _data.Allocate(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _data.Dispose();
        }

        public bool Contains(TValue item)
            => _data.Contains(item);

        public void Add(TValue item)
            => _data.Add(item);

        public bool Remove(TValue item)
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
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();

        object ICollection.Get(uint index)
            => _data.Get(index);
        
        void ICollection.Set(uint index, object value)
        {
#if !ANOTHERECS_RELEASE
            if (value == null || typeof(TValue) != value.GetType())
            {
                throw new ArgumentException(nameof(value));
            }
#endif
            _data.Set(index, (TValue)value);
        }

#if !ANOTHERECS_RELEASE
        private void Validate()
        {
            if (!ComponentUtils.IsSimple(typeof(TValue)))
            {
                throw new DArraySimpleException(typeof(TValue));
            }
        }
#endif

        private struct HashProvider : IHashProvider<TValue, uint>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetHash(ref TValue key)
                => (uint)key.GetHashCode();
        }

        public struct Enumerator : IEnumerator<TValue>, IEnumerator, IDisposable
        {
            private DHashSet<TValue> _data;
            private NHashSet<AllocatorSelector, TValue, HashProvider>.Enumerator _enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref DHashSet<TValue> data)
            {
                _data = data;
                _enumerator = data._data.GetEnumerator();

                if (_data.Count != 0)
                {
                    _data.EnterCheckChanges();
                }
            }

            public bool IsValid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.IsValid;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => _enumerator.MoveNext();

            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _enumerator.Current;
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
                if (_data.Count != 0)
                {
                    ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
                }
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
