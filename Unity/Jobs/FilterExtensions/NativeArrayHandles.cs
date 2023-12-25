using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using AnotherECS.Core.Collection;
using AnotherECS.Core;
using AnotherECS.Collections;

namespace AnotherECS.Unity.Jobs
{
    public unsafe class NativeArrayHandles : IModuleData, IDisposable
    {
        public const uint USER_DATA_ID = 0;

        private State _state;
        private NArray<BAllocator, Handles> _handles;

        public NativeArrayHandles(State state)
        {
            _state = state;
            _handles = new NArray<BAllocator, Handles>(&state.GetGlobalDepencies()->bAllocator, 1);
        }

        public NativeArray<TData> GetNativeArray<T, TData>(byte subId, WArray<TData> array)
            where T : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArray<T, TData>(subId, array.GetPtr(), array.Length);

        public NativeArray<TData> GetNativeArray<T, TData>(byte subId, NArray<BAllocator, TData> array)
            where T : unmanaged, IComponent
            where TData : unmanaged
            => GetNativeArray<T, TData>(subId, array.ReadPtr(), array.Length);

        public NativeArray<TData> GetNativeArray<T, TData>(byte subId, void* ptr, uint length)
            where T : unmanaged, IComponent
            where TData : unmanaged
        {
            var nativeArray = NativeArrayUtils.ToNativeArray<TData>(ptr, length);


            var data = GetByID(_state.GetIdByType<T>(), subId, ptr, length);

            //AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(data.safety);
            //AtomicSafetyHandle.UseSecondaryVersion(ref data.safety);
            //AtomicSafetyHandle.SetAllowSecondaryVersionWriting(data.safety, false);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, data.safety);

            return nativeArray;
        }

        public void Dispose()
        {
            _handles.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Handle GetByID(ushort id, byte subId, void* pointer, uint size)
        {
            if (id >= _handles.Length)
            {
                _handles.Resize(id + 1u);
            }

            var handle = _handles.GetRef(id).Get(subId);
            if (handle.pointer == null)
            {
                handle.safety = AtomicSafetyHandle.Create();
                handle.pointer = pointer;
                handle.size = size;
            }
            else if (handle.pointer != pointer || handle.size != size)
            {
                handle.Dispose();
                handle.safety = AtomicSafetyHandle.Create();
                handle.pointer = pointer;
                handle.size = size;
            }

            _handles.GetRef(id).Set(subId, handle);

            return handle;
        }

        private unsafe struct Handles : IDisposable
        {
            FArray4<Handle> data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Handle Get(uint id)
                => data[id];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Handle Set(uint id, Handle value)
                => data[id] = value;

            public void Dispose()
            {
                for (uint i = 0; i < data.Length; i++)
                {
                    Get(i).Dispose();
                }
            }
        }

        private unsafe struct Handle : IDisposable
        {
            public AtomicSafetyHandle safety;
            public void* pointer;
            public uint size;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (pointer != null)
                {
                    AtomicSafetyHandle.CheckDeallocateAndThrow(safety);
                    AtomicSafetyHandle.Release(safety);
                    this = default;
                }
            }
        }
    }
}