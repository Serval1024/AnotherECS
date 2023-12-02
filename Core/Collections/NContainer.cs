using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NContainer<T> : IDisposable, ISerialize
        where T : unmanaged
    {
        private T* _data;

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate()
        {
            Deallocate();
            _data = UnsafeMemory.Allocate<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref T data)
        {
            Allocate();
            Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(T data)
        {
            Allocate(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            UnsafeMemory.Deallocate(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() 
        {
            Deallocate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ref T data)
        {
            *GetPtr() = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T data)
        {
            Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
            => *GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef()
            => ref *GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNContainerBroken(this);
#endif
            return _data;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(GetRef());      //TODO SER null ptr is null ref?
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Allocate(reader.Unpack<T>());
        }
    }

}
