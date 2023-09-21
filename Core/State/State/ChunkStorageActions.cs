using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class ChunkStorageActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocateLayout(ref UnmanagedLayout<T> layout, uint denseCapacity, uint recycledCapacity)
        {
            ref var storage = ref layout.storage;
            storage.dense = ArrayPtr.Create<T>(denseCapacity);
            storage.recycle = ArrayPtr.Create<uint>(recycledCapacity);
            storage.denseIndex = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add(ref UnmanagedLayout<T> layout)
        {
            TryResizeDense(ref layout);
            return UnsafeAdd(ref layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref UnmanagedLayout<T> layout)
            => MultiStorageActions<T>.TryResizeDense(ref layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint UnsafeAdd(ref UnmanagedLayout<T> layout)
        {
            ref var storage = ref layout.storage;
            ref var recycleIndex = ref storage.recycleIndex;
            if (recycleIndex > 0)
            {
//#if !ANOTHERECS_HISTORY_DISABLE
                //_history.PushRecycledCount(recycledCount);
//#endif
                return storage.recycle.Get<uint>(--recycleIndex);
            }
            else
            {
                ref var denseIndex = ref storage.denseIndex;
//#if !ANOTHERECS_HISTORY_DISABLE
                //_history.PushCount(denseIndex);
//#endif
                return denseIndex++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Read(ref UnmanagedLayout<T> layout, uint id)
            => layout.storage.dense.GetPtr<T>(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UChunk* ReadAs<UChunk>(ref UnmanagedLayout<T> layout, uint id)
            where UChunk : unmanaged
            => layout.storage.dense.GetPtr<UChunk>(id);
       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref UnmanagedLayout<T> layout, uint id)
        {
            ref var storage = ref layout.storage;
            ref var component = ref storage.dense.GetRef<T>(id);
            ref var recycle = ref storage.recycle;
#if !ANOTHERECS_HISTORY_DISABLE
            //_history.PushRecycledCount(recycle);
            //_history.PushRecycled(_recycled[recycle], recycle);
#endif
            recycle.Set(storage.recycleIndex++, id);
        }
    }

  






#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
        public static unsafe class StorageActions2<T>
        where T : unmanaged, IComponent
    {
      

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init(ref StorageLayout<T> layout, ref GlobalDepencies depencies)
        {
            ref var storage = ref layout.storage;
            storage.sparse = UnsafeMemory.Allocate<ushort>(32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHas(ref StorageLayout<T> layout, EntityId id)
            => ((ushort*)layout.storage.sparse)[id] != 0;

        */


        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
        {

            return false;

        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ref T data)
        {
            ref var component = ref AddInternal(id);
            component = data;

        }

        public ref T Add(EntityId id)
        {
            ref var component = ref AddInternal(id);

            return ref component;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        private ref T AddInternal(EntityId id)

        {

            ushort denseIndex;


            ref var recycledCount = ref _data.recycle;
            if (recycledCount > 0)
            {

                _history.PushRecycledCount(recycledCount);

                denseIndex = _recycled[--recycledCount];
            }
            else


            {
                ref var currentIndex = ref _data.index;
#if ANOTHERECS_DEBUG
                if (currentIndex == ushort.MaxValue)
                {
                    throw new Exceptions.ReachedLimitComponentException(ushort.MaxValue);
                }
#endif
                denseIndex = currentIndex;

                _history.PushCount(currentIndex);

                ++currentIndex;
            }




            _history.PushSparse(false, id);



            _sparse[id] = true;




            return ref _dense[denseIndex];

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read(EntityId id)

            => ref _dense[id];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(EntityId id)
        {

            var denseIndex = (ushort)id;

            ref var component = ref _dense[denseIndex];


            return ref component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(EntityId id, ref T data)
        {

            var denseIndex = (ushort)id;

            ref var component = ref _dense[denseIndex];

            component = data;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRecycledResized()
        {
            if (_data.recycle == _recycled.Length)
            {
                Array.Resize(ref _recycled, _data.recycle << 1);
                return true;
            }
            return false;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id)
        {

            var denseIndex = (ushort)id;


            ref var component = ref _dense[denseIndex];


            ref var recycledCount = ref _data.recycle;




            _history.PushRemove(denseIndex, id, _recycled[recycledCount], recycledCount);








            component = default;



            _recycled[recycledCount++] = denseIndex;



            _sparse[id] = false;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int capacity)
        {
            Array.Resize(ref _sparse, capacity);


            Array.Resize(ref _dense, capacity);



        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetTypeId()
            => _elementId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => _elementType;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushChanges()
            => _history.PushChange(_dense, _data.index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Reset();

            Array.Clear(_dense, 1, _data.index);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {



            _data.index = 1;


            Array.Clear(_recycled, 0, _data.recycle);
            _data.recycle = 0;

            Array.Clear(_sparse, 1, _sparse.Length - 1);
        }





        public void SetRecycledCountRaw(ushort count)

            => _data.recycle = count;
        

        public void SetCountRaw(ushort count)

            => _data.index = count;
        

        public ushort[] GetRecycledRaw()

            => _recycled;


        public void SetDenseRaw(T[] data)

            => _dense = data;

        public bool[] GetSparseRaw()
            => _sparse;

        public T[] GetDenseRaw()

            => _dense;



        public T Create()
        {

            return default;

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
            => _history.RevertTo(tick, ref this);


        public void Pack(ref WriterContextSerializer writer)
        {

            writer.WriteUnmanagedArray(_sparse);


            writer.WriteUnmanagedArray(_dense, _data.index);



            writer.WriteUnmanagedArray(_recycled);


            writer.WriteStruct(_data);

            writer.Write(_elementId);

        }

        public void Unpack(ref ReaderContextSerializer reader)
        {

            _sparse = reader.ReadUnmanagedArray<bool>();


            _dense = reader.ReadUnmanagedArray<T>();



            _recycled = reader.ReadUnmanagedArray<ushort>();


            _data = reader.ReadStruct<IndexData>();

            _elementId = reader.ReadUInt16();

            _elementType = typeof(T);
        }



        private class IndexData

        {

            public ushort recycle;

            public ushort index;

            public void Pack(ref WriterContextSerializer writer)
            {

                writer.Write(recycle);

                writer.WriteStruct(index);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {

                recycle = reader.ReadUInt16();

                index = reader.ReadStruct<ushort>();
            }
        }*/

    }


}

