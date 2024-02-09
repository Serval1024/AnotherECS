using AnotherECS.Collections.Exceptions;
using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [ForceBlittable]
    public unsafe struct DDictionary<TKey, TValue> : IInject<WPtr<AllocatorSelector>>, IEnumerable<Pair<TKey, TValue>>, ICollection, IValid, ISerialize, IRepairMemoryHandle
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private NDictionary<AllocatorSelector, TKey, TValue, HashProvider> _data;

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        internal DDictionary(AllocatorSelector* allocator)
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
                throw new MissInjectException(typeof(DDictionary<TKey, TValue>));
            }

            _data.Allocate(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _data.Dispose();
        }

        public bool ContainsKey(TKey key)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _data.Add(key, value);
        }

        public bool Remove(TKey key)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.Remove(key);
        }

        public object Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif
            return _data.Get(index);
        }

        public void Set(uint index, object value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (value == null || typeof(TValue) != value.GetType())
            {
                throw new ArgumentException(nameof(value));
            }
            if (index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
#endif
            _data.Set(index, (TValue)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
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
        IEnumerator<Pair<TKey, TValue>> IEnumerable<Pair<TKey, TValue>>.GetEnumerator()
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
            if (!ComponentUtils.IsSimple(typeof(TKey)))
            {
                throw new DArraySimpleException(typeof(TKey));
            }
            if (!ComponentUtils.IsSimple(typeof(TValue)))
            {
                throw new DArraySimpleException(typeof(TValue));
            }
        }
#endif

        private struct HashProvider : IHashProvider<TKey, uint>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetHash(ref TKey key)
                => (uint)key.GetHashCode();
        }

        public struct Enumerator : IEnumerator<Pair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private DDictionary<TKey, TValue> _data;
            private NDictionary<AllocatorSelector, TKey, TValue, HashProvider>.Enumerator _enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ref DDictionary<TKey, TValue> data)
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

            public Pair<TKey, TValue> Current
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
