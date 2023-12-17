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
    public unsafe struct DArray<T> : IInject<NPtr<HAllocator>>, IEnumerable<T>, ISerialize, ICArray, IRebindMemoryHandle
        where T : unmanaged
    {
        private NArray<HAllocator, T> _data;
        private HAllocator* _allocator;

        internal bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        internal DArray(HAllocator* allocator)
        {
            _data = default;
            _allocator = allocator;
#if !ANOTHERECS_RELEASE
            Validate();
#endif
        }

#if !ANOTHERECS_RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject<NPtr<HAllocator>>.Construct(NPtr<HAllocator> allocator)
        {
            _allocator = allocator.Value;
            Validate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IInject.Deconstruct()
        { 
            Deallocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            if (_data.IsValide)
            {
                MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
            }
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct(NPtr<HAllocator> allocator)
        {
            _allocator = allocator.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct()
        {
            Deallocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }
#endif
        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValide;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint length)
        {
            if (!_allocator->IsValide)
            {
                throw new MissInjectException(typeof(DArray<T>));
            }

            if (length >= 0)
            {
                Deallocate();
                _data.Allocate(_allocator, length);
            }
            else
            {
                throw new ArgumentException($"Argument '{nameof(length)}', must be equals or greater 0.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            if (_data.IsValide)
            {
                _data.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint capacity)
        {
            _data.Resize(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read(uint index)
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(this.GetType());
            }
            return ref _data.ReadRef(index);
        }
#else
            => ref _data.ReadRef(index);
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(uint index)
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(GetType());
            }
            return ref _data.GetRef(index);
        }
#else
            => ref _data.GetRef(index);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
            Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref T value)
#if !ANOTHERECS_RELEASE
        {
            if (!IsValide)
            {
                throw new DArrayInvalideException(GetType());
            }
            _data.Set(index, ref value);
        }
#else
            => _data.Set(index, ref value);
#endif

        public int IndexOf(T item)
            => IndexOf(ref item, _data.Length);
        
        public bool Contains(T item)
            => IndexOf(ref item, _data.Length) != -1;

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, Length);
        }

        public void CopyTo(T[] array, uint arrayIndex)
        {
            CopyTo(array, arrayIndex, _data.Length);
        }

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
            => new(ReadPtr(), (int)Length);

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

        object ICArray.Get(uint index)
            => Get(index);

        void ICArray.Set(uint index, object value)
        {
            Set(index, (T)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
            => new Enumerator(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
            => _data.ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CopyTo(T[] array, uint startIndex, uint count)
        {
            _data.CopyTo(array, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe int IndexOf(ref T item, uint count)
        {
#if !ANOTHERECS_RELEASE
            if (!IsValide)
            {
                throw new DArrayInvalideException(this.GetType());
            }
#endif
            var array = ReadPtr();

            var comparer = EqualityComparer<T>.Default;
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
        internal unsafe T* ReadPtr()
            => _data.ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe T* GetPtr()
            => _data.GetPtr();

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


        public struct Enumerator : IEnumerator<T>
        {
            private readonly DArray<T> _data;
            private uint _current;
            private readonly uint _length;

            public Enumerator(ref DArray<T> data)
            {
                _data = data;
                _length = _data.Length;
                _current = uint.MaxValue;

                _data.EnterCheckChanges();
            }

            public T Current
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
                ExceptionHelper.ThrowIfChange(_data.ExitCheckChanges());
            }
        }
    }
}
