﻿using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct DefaultFeature<TAllocator, TDense> : IData<TAllocator>, IDefaultSetter<TDense>
        where TAllocator : unmanaged, IAllocator
        where TDense : struct, IDefault
    {
        public State state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(State state, Dependencies* dependencies, ushort callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>
        {
            this.state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDefault(ref TDense component)
        {
            component.Setup(state);
        }
    }
}
