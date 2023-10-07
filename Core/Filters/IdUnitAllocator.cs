using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct IdUnitAllocator : ISerialize
    {
        private uint _counter;
        private uint _recycle;
        private uint[] _recycled;


        //public IdAllocator(uint bufferSize, uint segmentSize, uint recycledCapacity, in HistoryByChangeArgs args)
        public IdUnitAllocator(uint capacity)
        {
            _recycled = new uint[capacity];
            _recycle = 0;
            _counter = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Allocate()
        {
            if (_recycle > 0)
            {
//#if !ANOTHERECS_HISTORY_DISABLE
                //_history.PushRecycledCount(_recycle);
//#endif
                return _recycled[--_recycle];
            }
            else
            {
//#if !ANOTHERECS_HISTORY_DISABLE
                //_history.PushCount(_counter);
//#endif
                return _counter++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocatedCount()
            => GetAllocatedUpperBoundId() - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocatedUpperBoundId()
            => _counter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(uint id)
        {
            _recycled[_recycle++] = id;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_counter);
            writer.Write(_recycle);
            writer.WriteUnmanagedArray(_recycled);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _counter = reader.ReadUInt32();
            _recycle = reader.ReadUInt32();
            _recycled = reader.ReadUnmanagedArray<uint>(); 
        }
    }
}
