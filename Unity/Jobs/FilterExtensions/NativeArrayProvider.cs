using AnotherECS.Core;
using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Unity.Jobs
{
    public unsafe class NativeArrayProvider : IDisposable
    {
        private const uint DATA_COUNT_PER_COMPONENT = 3;

        public NativeArray<uint> UintDummy { get; private set; }

        private readonly State _state;
        private NArray<BAllocator, NativeArrayStorage<Dummy>> _byDenseIds;
        private NArray<BAllocator, NativeArrayStorage<Dummy>> _byComponents;
        private NDictionary<BAllocator, uint, NativeArrayStorage<Dummy>, U4U4HashProvider> _byIds;

        
        public NativeArrayProvider(State state)
        {
            _state = state;
            _byDenseIds = new NArray<BAllocator, NativeArrayStorage<Dummy>>(&state.GetDependencies()->bAllocator, 1);
            _byComponents = new NArray<BAllocator, NativeArrayStorage<Dummy>>(&state.GetDependencies()->bAllocator, 1);
            _byIds = new NDictionary<BAllocator, uint, NativeArrayStorage<Dummy>, U4U4HashProvider>(&state.GetDependencies()->bAllocator, 1);
            UintDummy = new NativeArray<uint>(0, Allocator.Persistent);
        }

        public NativeArray<TData> GetNativeArrayById<TData>(uint id, WArray<TData> array)
            where TData : unmanaged
        {
            var dataPtr = _byIds.TryGetPtrValue(id);
            if (dataPtr == null)
            {
                _byIds.Add(id, new NativeArrayStorage<Dummy>());
                dataPtr = _byIds.TryGetPtrValue(id);
            }

            return TryPrepareStorage(dataPtr, array.GetPtr(), array.Length)->nativeArray;
        }

        public NativeArray<TData> GetNativeArrayByDenseId<TData>(uint id, WArray<TData> array)
            where TData : unmanaged
            => GetNativeArray(ref _byDenseIds, id, array.GetPtr(), array.Length);

        public NativeArray<TData> GetNativeArrayByComponent<TComponent, TData>(byte subId, WArray<TData> array)
            where TComponent : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArrayByComponent<TComponent, TData>(subId, array.GetPtr(), array.Length);

        public void Dispose()
        {
            _byDenseIds.DeepDispose();
            _byComponents.DeepDispose();
            _byIds.DeepDispose();
            UintDummy.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArray<TData> GetNativeArrayByComponent<T, TData>(byte subId, TData* ptr, uint length)
            where T : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArray<TData>(ref _byComponents, _state.GetIdByType<T>() * DATA_COUNT_PER_COMPONENT + subId, ptr, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArray<TData> GetNativeArray<TData>(ref NArray<BAllocator, NativeArrayStorage<Dummy>> collection, uint id, TData* ptr, uint length)
            where TData : unmanaged
            => (ptr != null)
                ? GetStorage(ref collection, id, ptr, length).nativeArray
                : default;

        private struct Dummy { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref NativeArrayStorage<TData> GetStorage<TData>(ref NArray<BAllocator, NativeArrayStorage<Dummy>> collections, uint id, TData* ptr, uint length)
            where TData : unmanaged
        {
            if (id >= collections.Length)
            {
                collections.Resize(id + 1u);
            }

            return ref *TryPrepareStorage(collections.GetPtr(id), ptr, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArrayStorage<TData>* TryPrepareStorage<TData>(NativeArrayStorage<Dummy>* storagePtr, TData* ptr, uint length)
            where TData : unmanaged
        {
            var storage = (NativeArrayStorage<TData>*)storagePtr;
            if (!storage->nativeArray.IsCreated)
            {
                FillStorage(ref *storage, ptr, length);
            }
            else if (storage->nativeArray.GetUnsafePtr() != ptr || storage->nativeArray.Length != length)
            {
                storage->Dispose();
                FillStorage(ref *storage, ptr, length);
            }

            storage->tick = _state.Tick;

            return storage;
        }

        private void FillStorage<TData>(ref NativeArrayStorage<TData> storage, void* ptr, uint length)
            where TData : unmanaged
        {
            storage.nativeArray = NativeArrayUtils.ToNativeArray<TData>(ptr, length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            storage.safety = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref storage.nativeArray, storage.safety);
#endif
        }

        private unsafe struct NativeArrayStorage<T> : IDisposable
            where T : struct
        {
            public uint tick;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            public AtomicSafetyHandle safety;
#endif
            public NativeArray<T> nativeArray;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (nativeArray.IsCreated)
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    AtomicSafetyHandle.CheckDeallocateAndThrow(safety);
                    AtomicSafetyHandle.Release(safety);
#endif
                    this = default;
                }
            }
        }
    }
}