using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct URecycle<TNumber, TNumberProvider>: IRepairMemoryHandle, ISerialize
        where TNumber : unmanaged
        where TNumberProvider : struct, INumberProvier<TNumber>
    {
        public NArray<HAllocator, TNumber> _data;
        public uint _currentIndex;
        public uint _counter;

        public URecycle(HAllocator* allocator, uint capacity)
        {
            _data = new NArray<HAllocator, TNumber>(allocator, capacity);
            _currentIndex = default;
            _counter = default;
            _counter = GetStartIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetStartIndex()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated()
            => _counter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => _counter - GetStartIndex() - _currentIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TNumber Allocate()
        {
            if (_currentIndex > 0)
            {
                return _data.ReadRef(--_currentIndex);
            }
            else
            {
#if !ANOTHERECS_RELEASE
                LayoutActions.CheckLimit<TNumber, TNumberProvider>(_counter);
#endif
                return default(TNumberProvider).ToGeneric(_counter++);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Deallocate(TNumber id)
        {
            if (_currentIndex == _data.Length)
            {
                _data.Resize(_data.Length << 1);
            }

            _data.GetRef(_currentIndex++) = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _data, ref repairMemoryContext);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _data.PackBlittable(ref writer);
            writer.Write(_currentIndex);
            writer.Write(_counter);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.UnpackBlittable(ref reader);
            _currentIndex = reader.ReadUInt32();
            _counter = reader.ReadUInt32();
        }
    }
}