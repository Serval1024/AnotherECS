using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct Adapters : ISerialize
    {
        private IAdapter[] _adapters;

        public Adapters(IAdapter[] adapters)
        {
            _adapters = adapters;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adapters.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAdapter[] Gets()
            => _adapters;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAdapter Get(int typeId)
            => _adapters[typeId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IStorage[] GetStorages()
        {
            var storages = new IStorage[_adapters.Length];
            for (int i = 0; i < _adapters.Length; ++i)
            {
                storages[i] = _adapters[i].GetStorage();
            }
            return storages;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IStorage GetStorage(ushort typeId)
          => _adapters[typeId].GetStorage();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCanAsEntity(ushort typeId)
            => _adapters[typeId] is IEntityAdapter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCanAsEntity<T>(ushort typeId)
            where T : struct, IComponent
            => _adapters[typeId] is IEntityAdapter<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntityAdapter GetAsEntity(ushort typeId)
#if ANOTHERECS_DEBUG
            => (IEntityAdapter)_adapters[typeId];
#else
            => UnsafeUtility.As<IAdapter, IEntityAdapter>(ref _adapters[typeId]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntityAdapter<T> GetAsEntity<T>(ushort typeId)
            where T : struct, IComponent
#if ANOTHERECS_DEBUG
            => (IEntityAdapter<T>)_adapters[typeId];
#else
            => UnsafeUtility.As<IAdapter, IEntityAdapter<T>>(ref _adapters[typeId]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntityAdapterAdd<T> GetAsEntityAdd<T>(ushort typeId)
            where T : struct, IComponent
#if ANOTHERECS_DEBUG
            => (IEntityAdapterAdd<T>)_adapters[typeId];
#else
            => UnsafeUtility.As<IAdapter, IEntityAdapterAdd<T>>(ref _adapters[typeId]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCanAsSingle(ushort typeId)
            => _adapters[typeId] is ISingleAdapter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCanAsSingle<T>(ushort typeId)
            where T : struct, IComponent
            => _adapters[typeId] is ISingleAdapter<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ISingleAdapter GetAsSingle(ushort typeId)
#if ANOTHERECS_DEBUG
            => (ISingleAdapter)_adapters[typeId];
#else
            => UnsafeUtility.As<IAdapter, ISingleAdapter>(ref _adapters[typeId]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ISingleAdapter<T> GetAsSingle<T>(ushort typeId)
            where T : struct, IComponent
#if ANOTHERECS_DEBUG
            => (ISingleAdapter<T>)_adapters[typeId];
#else
            => UnsafeUtility.As<IAdapter, ISingleAdapter<T>>(ref _adapters[typeId]);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rebind(ushort typeId, IEntityStorage storage)
        {
            _adapters[typeId].Rebind(storage);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int capacity)
        {   
            for (int i = 0; i < _adapters.Length; ++i)
            {
                if (_adapters[i] is IEntityAdapter entityAdapter)
                {
                    entityAdapter.Resize(capacity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        { 
            Array.Clear(_adapters, 1, _adapters.Length - 1);
        }

        public void Dispose()
        {
            for(int i = 1; i < _adapters.Length; ++i)
            {
                if (_adapters[i] is IDisposableInternal disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void Recycle()
        {
            for (int i = 1; i < _adapters.Length; ++i)
            {
                if (_adapters[i] is IRecycleInternal recycle)
                {
                    recycle.Recycle();
                }
            }
            Clear();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(_adapters);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _adapters = reader.Unpack<IAdapter[]>();
        }
    }
}

