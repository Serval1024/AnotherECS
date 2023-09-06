using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    public unsafe struct PutFuctions<T>
        where T : State
    {
        private readonly Action<T, IAdapterReference>[] _functions;

        public PutFuctions(int count)
        {
            _functions = new Action<T, IAdapterReference>[count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int index, T state, IAdapterReference adapter)
        {
            _functions[index](state, adapter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, Action<T, IAdapterReference> function)
        {
            _functions[index] = function;
        }
    }
}
