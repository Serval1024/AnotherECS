using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NBuffer<TAllocator, T> : INative, ISerialize, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        private NList<TAllocator, T> _data;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }

        public bool IsEmpty
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NBuffer(TAllocator* allocator, uint capacity)
        {
            _data = new NList<TAllocator, T>(allocator, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T element)
        {
            _data.Add(element);
        }

        public T Pop()
        {
#if !ANOTHERECS_RELEASE
            if (IsEmpty)
            {
                throw new InvalidOperationException();
            }
#endif
            _data.RemoveLast();
            return *(_data.ReadPtr() + _data.Count);
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
        public void PackBlittable(ref WriterContextSerializer writer)
        {
            _data.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            _data.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }
    }
}
