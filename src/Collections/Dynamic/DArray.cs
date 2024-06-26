﻿using AnotherECS.Collections.Exceptions;
using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Jobs")]
namespace AnotherECS.Collections
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [ForceBlittable]
    public unsafe struct DArray<TValue> : IInject<WPtr<AllocatorSelector>>, IEnumerable<TValue>, ISerialize, ICollection, IValid, IRepairMemoryHandle, IRepairStateId
        where TValue : unmanaged
    {
        private NArray<AllocatorSelector, TValue> _data;

        internal DArray(AllocatorSelector* allocator)
        {
            _data = default;
            _data.SetAllocator(allocator);
#if !ANOTHERECS_RELEASE
            Validate();
#endif
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }

        uint ICollection.Count => Length;

        internal uint Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.GetMemoryHandle().id;
        }

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint length)
        {
            if (!_data.IsAllocatorValid())
            {
                throw new MissInjectException(typeof(DArray<TValue>));
            }
            _data.Allocate(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint capacity)
        {
            if (_data.IsValid)
            {
                _data.Resize(capacity);
            }
            else
            {
                _data.Allocate(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return ref _data.ReadRef(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return ref _data.GetRef(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, TValue value)
        {
            Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref TValue value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _data.Set(index, ref value);
        }

        public int IndexOf(TValue item)
            => IndexOf(ref item, _data.Length);
        
        public bool Contains(TValue item)
            => IndexOf(ref item, _data.Length) != -1;

        public void CopyFrom(DArray<TValue> source)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(source);
#endif
            if (Length != source.Length)
            {
                Allocate(source.Length);
            }

            source._data.CopyFrom(source._data);
        }

        public void CopyTo(TValue[] array)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            CopyTo(array, 0, Length);
        }

        public void CopyTo(TValue[] array, uint arrayIndex)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            CopyTo(array, arrayIndex, _data.Length);
        }

        public TValue this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, ref value);
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
        public unsafe Span<TValue> AsSpan()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return new(ReadPtr(), (int)Length);
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

        object ICollection.Get(uint index)
            => Read(index);

        void ICollection.Set(uint index, object value)
        {
            Set(index, (TValue)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TValue> GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] ToArray()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CopyTo(TValue[] array, uint startIndex, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            _data.CopyTo(array, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe int IndexOf(ref TValue item, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            var array = ReadPtr();

            var comparer = EqualityComparer<TValue>.Default;
            for (int i = 0; i < count; ++i)
            {
                if (comparer.Equals(array[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<TValue> ToWArray()
            => _data.ToWArray(0, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WArray<TValue> ToWArray(uint start, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data.ToWArray(start, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TValue* ReadPtr()
            => _data.ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TValue* GetPtr()
            => _data.GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnterCheckChanges()
            => _data.EnterCheckChanges();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ExitCheckChanges()
            => _data.ExitCheckChanges();


        #region inner interfaces
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

        bool IRepairStateId.IsRepairStateId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => typeof(IRepairStateId).IsAssignableFrom(typeof(TValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairStateId.RepairStateId(ushort stateId)
        {
            if (IsValid)
            {
                RepairIdElement(stateId, 0, Length);
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RepairIdElement(ushort stateId, uint start, uint elementCount)
        {
            if (typeof(IRepairStateId).IsAssignableFrom(typeof(TValue)))
            {
                for (uint i = start; i < elementCount; ++i)
                {
                    var data = (IRepairStateId)_data.ReadRef(i);
                    data.RepairStateId(stateId);
                    _data.ReadRef(i) = (TValue)data;
                }
            }
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

        #region declarations
        public struct Enumerator : IEnumerator<TValue>
        {
            private readonly DArray<TValue> _data;
            private uint _current;
            private readonly uint _length;

            public Enumerator(ref DArray<TValue> data)
            {
                _data = data;
                _length = _data.Length;
                _current = uint.MaxValue;

                if (_length != 0)
                {
                    _data.EnterCheckChanges();
                }
            }

            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.Read(_current);
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_current < _length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (_length != 0)
                {
                    ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
                }
            }
        }
        #endregion
    }
}
