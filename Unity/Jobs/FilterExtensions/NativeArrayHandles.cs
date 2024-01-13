using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using AnotherECS.Core.Collection;
using AnotherECS.Core;

namespace AnotherECS.Unity.Jobs
{
    public unsafe class NativeArrayHandles : IModuleData, IDisposable
    {
        public const uint MODULE_DATA_ID = 0;
        private const uint DATA_COUT_PER_COMPONENT = 3;

        public NativeArray<uint> UintDummy { get; private set; }

        private readonly State _state;
        private NArray<BAllocator, Handle<Dummy>> _byIds;
        private NArray<BAllocator, Handle<Dummy>> _byComponents;

        public NativeArrayHandles(State state)
        {
            _state = state;
            _byIds = new NArray<BAllocator, Handle<Dummy>>(&state.GetDependencies()->bAllocator, 1);
            _byComponents = new NArray<BAllocator, Handle<Dummy>>(&state.GetDependencies()->bAllocator, 1);
            UintDummy = new NativeArray<uint>(0, Allocator.Persistent);
        }

        public NativeArray<TData> GetNativeArrayById<TData>(uint id, WArray<TData> array)
            where TData : unmanaged
            => GetNativeArray<TData>(ref _byIds, id, array.GetPtr(), array.Length);

        public NativeArray<TData> GetNativeArrayByComponent<T, TData>(byte subId, WArray<TData> array)
            where T : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArrayByComponent<T, TData>(subId, array.GetPtr(), array.Length);

        public void Dispose()
        {
            _byIds.Dispose();
            _byComponents.Dispose();
            UintDummy.Dispose();
        }

        private NativeArray<TData> GetNativeArrayByComponent<T, TData>(byte subId, void* ptr, uint length)
            where T : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArray<TData>(ref _byComponents, _state.GetIdByType<T>() * DATA_COUT_PER_COMPONENT + subId, ptr, length);

        private NativeArray<TData> GetNativeArray<TData>(ref NArray<BAllocator, Handle<Dummy>> collection, uint id, void* ptr, uint length)
            where TData : unmanaged
            => (ptr != null)
                ? GetHandle<TData>(ref collection, id, ptr, length).nativeArray
                : default;

        private struct Dummy { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Handle<TData> GetHandle<TData>(ref NArray<BAllocator, Handle<Dummy>> collections, uint id, void* ptr, uint length)
            where TData : unmanaged
        {
            if (id >= collections.Length)
            {
                collections.Resize(id + 1u);
            }

            ref var handle = ref *(Handle<TData>*)collections.GetPtr(id);
            if (!handle.nativeArray.IsCreated)
            {
                FillHandle(ref handle, ptr, length);
            }
            else if (handle.nativeArray.GetUnsafePtr() != ptr || handle.nativeArray.Length != length)
            {
                handle.Dispose();
                FillHandle(ref handle, ptr, length);
            }

            return ref handle;
        }

        private void FillHandle<TData>(ref Handle<TData> handle, void* ptr, uint length)
            where TData : unmanaged
        {
            handle.safety = AtomicSafetyHandle.Create();
            handle.nativeArray = NativeArrayUtils.ToNativeArray<TData>(ptr, length);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref handle.nativeArray, handle.safety);
        }
        
        private unsafe struct Handle<T> : IDisposable
            where T : struct
        {
            public AtomicSafetyHandle safety;
            public NativeArray<T> nativeArray;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (nativeArray.IsCreated)
                {
                    AtomicSafetyHandle.CheckDeallocateAndThrow(safety);
                    AtomicSafetyHandle.Release(safety);
                    this = default;
                }
            }
        }
    }
}